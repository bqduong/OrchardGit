using Magelia.WebStore.Contracts;
using Magelia.WebStore.Events;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.Basket;
using Magelia.WebStore.MVC;
using Magelia.WebStore.Services.Contract.Data.Store;
using Magelia.WebStore.Services.Contract.Parameters.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Magelia.WebStore.Controllers
{
    public class BasketController : Controller
    {
        private Localizer _localizer;
        private dynamic _shapeFactory;
        private IOrchardServices _orchardServices;
        private IWebStoreServices _webStoreServices;
        private IEnumerable<IBasketEventHandler> _basketEventHandlers;

        public BasketController(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IEnumerable<IBasketEventHandler> basketEventHandlers, IShapeFactory shapeFactory)
        {
            this._shapeFactory = shapeFactory;
            this._orchardServices = orchardServices;
            this._localizer = NullLocalizer.Instance;
            this._webStoreServices = webStoreServices;
            this._basketEventHandlers = basketEventHandlers;
        }

        [HttpGet]
        [NoCache]
        public ActionResult BasketCount(Int32 basketCountPartId)
        {
            BasketCountPart part = this._orchardServices.ContentManager.Get<BasketCountPart>(basketCountPartId, VersionOptions.Published);

            BasketCountViewModel viewModel = new BasketCountViewModel { BasketUrl = part.BasketUrl };

            this._webStoreServices.UsingClient(
                c =>
                {
                    BasketCountResult result = c.BasketClient.GetBasketProductsCount(this._webStoreServices.CurrentUserName, this._webStoreServices.BasketName);
                    switch (part.Count)
                    {
                        case 1:
                            viewModel.Count = result.DistinctProductsAndGiftsCount;
                            break;
                        case 2:
                            viewModel.Count = result.ProductsCount;
                            break;
                        case 3:
                            viewModel.Count = result.ProductsAndGiftsCount;
                            break;
                        default:
                            viewModel.Count = result.DistinctProductsCount;
                            break;
                    }
                }
            );

            return new ShapePartialResult(this, this._shapeFactory.DisplayTemplate(TemplateName: "Basket/BasketCount", Model: viewModel));
        }

        [HttpPost]
        public JsonResult AddToBasket(Guid productId, Int32 quantity)
        {
            Boolean success = false;

            Int32 addedQuantity = 0;
            Int32 totalQuantity = 0;

            if (quantity > 0)
            {
                Exception exception = this._webStoreServices.UsingClient(
                    c =>
                    {
                        Int32 initialQuantity = c.BasketClient.GetBaskets(this._webStoreServices.CurrentUserName, new[] { this._webStoreServices.BasketName })
                                                              .SelectMany(b => b.OrderSubsets)
                                                              .SelectMany(os => os.LineItems)
                                                              .Where(li => li.ProductId == productId)
                                                              .Sum(li => li.Quantity);

                        BasketEntryResult result = c.BasketClient.AddProductsToBasket(
                            this._webStoreServices.CurrentUserName,
                            this._webStoreServices.IsAnonymous,
                            this._webStoreServices.BasketName,
                            productId,
                            quantity,
                            this._webStoreServices.CurrentCurrencyId,
                            this._webStoreServices.CurrentCultureId,
                            new Location
                            {
                                RegionId = this._webStoreServices.CurrentRegionId,
                                CountryId = this._webStoreServices.CurrentCountryId
                            }
                        ).FirstOrDefault(ber => ber.ProductId == productId);

                        if (result == null)
                        {
                            success = false;
                        }
                        else
                        {
                            if (
                                result.State != BasketEntryState.Dispatched &&
                                result.State != BasketEntryState.PartiallyDispatched
                            )
                            {
                                success = true;
                            }
                            else
                            {
                                success = true;
                                totalQuantity = result.Quantity;
                                addedQuantity = totalQuantity - initialQuantity;
                            }

                            this._basketEventHandlers.Trigger(h => h.ProductAddedToBasket(result));
                        }
                    }
                );

                if (exception != null)
                {
                    success = false;
                }
            }

            return this.Json(new { success = success, totalQuantity = totalQuantity, addedQuantity = addedQuantity });
        }

        [HttpPost]
        public ActionResult UpdateProductQuantity(Int32 basketPartId, Guid orderSubsetId, Guid productId, Int32 quantity)
        {
            String message = null;
            if (quantity >= 0)
            {
                BasketEntryResult result = null;

                if (
                    this._webStoreServices.UsingClient(
                        c => result = c.BasketClient.UpdateProductQuantity(
                                this._webStoreServices.CurrentUserName,
                                this._webStoreServices.BasketName,
                                orderSubsetId,
                                productId,
                                quantity
                            ).FirstOrDefault(r => r.ProductId == productId)
                    ) != null
                )
                {
                    message = this._localizer("An unexpected error has occured").ToString();
                }
                else if (result != null)
                {
                    this._basketEventHandlers.Trigger(h => h.ProductQuantityUpdated(result));
                }
            }
            return this.GetBasket(basketPartId, null, message);
        }

        [HttpPost]
        public ActionResult AddPromoCode(Int32 basketPartId, String promoCode)
        {
            String message = null;
            String currentPromoCode = null;

            if (!String.IsNullOrEmpty(promoCode))
            {
                PromoCodeEntryResult result = null;

                Exception exception = this._webStoreServices.UsingClient(
                    c =>
                    {
                        result = c.BasketClient.ApplyPromoCodes(this._webStoreServices.CurrentUserName, this._webStoreServices.BasketName, new[] { promoCode }).FirstOrDefault(r => String.Equals(r.PromoCode, promoCode, StringComparison.OrdinalIgnoreCase));

                        if (result == null || result.State != PromoCodeEntryState.Added)
                        {
                            currentPromoCode = promoCode;
                            message = this._localizer("Promo code is not applicable").ToString();
                        }
                    }
                );

                if (exception != null)
                {
                    currentPromoCode = promoCode;
                    message = this._localizer("An unexpected error has occured").ToString();
                }
                else if (result != null)
                {
                    this._basketEventHandlers.Trigger(h => h.PromoCodeAdded(result));
                }
            }

            return this.GetBasket(basketPartId, currentPromoCode, message);
        }

        [HttpPost]
        public ActionResult RemovePromoCode(Int32 basketPartId, String promoCode)
        {
            String message = null;

            if (!String.IsNullOrEmpty(promoCode))
            {
                PromoCodeEntryResult result = null;

                if (this._webStoreServices.UsingClient(c => result = c.BasketClient.RemovePromoCodes(this._webStoreServices.CurrentUserName, this._webStoreServices.BasketName, promoCode).FirstOrDefault(r => String.Equals(r.PromoCode, promoCode, StringComparison.OrdinalIgnoreCase))) != null)
                {
                    message = this._localizer("An unexpected error has occured").ToString();
                }
                else if (result != null)
                {
                    this._basketEventHandlers.Trigger(h => h.PromoCodeRemoved(result));
                }
            }

            return this.GetBasket(basketPartId, null, message);
        }

        [HttpGet]
        [NoCache]
        public ActionResult UpdateBasket(Int32 basketPartId)
        {
            String message = null;

            Exception exception = this._webStoreServices.UsingClient(
                c =>
                {
                    if (
                        c.BasketClient.UpdateBasket(
                            this._webStoreServices.CurrentUserName,
                            this._webStoreServices.BasketName,
                            this._webStoreServices.CurrentCurrencyId,
                            CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).LCID,
                            new Location { CountryId = this._webStoreServices.CurrentCountryId, RegionId = this._webStoreServices.CurrentRegionId }
                        )
                        .Any(ber => ber.State != BasketEntryState.Dispatched))
                    {
                        message = this._localizer("Some products are no more available").ToString();
                    }
                }
            );

            if (exception != null)
            {
                message = this._localizer("An unexpected error has occured").ToString();
            }

            return this.GetBasket(basketPartId, null, message);
        }

        [HttpGet]
        [NoCache]
        public ActionResult ClearBasket(Int32 basketPartId)
        {
            String message = null;
            String basketName = null;

            if (this._webStoreServices.UsingClient(c => basketName = c.BasketClient.DeleteBaskets(this._webStoreServices.CurrentUserName, new[] { this._webStoreServices.BasketName }).FirstOrDefault()) != null)
            {
                message = this._localizer("An unexpected error has occured").ToString();
            }

            if (String.Equals(this._webStoreServices.BasketName, basketName, StringComparison.OrdinalIgnoreCase))
            {
                this._basketEventHandlers.Trigger(h => h.Cleared());
            }

            return this.GetBasket(basketPartId, null, message);
        }

        [HttpGet]
        [NoCache]
        public ActionResult GetBasket(Int32 basketPartId, String currentPromoCode, String message)
        {
            BasketPart basketPart = this._orchardServices.ContentManager.Get<BasketPart>(basketPartId, VersionOptions.Published);

            BasketViewModel viewModel = new BasketViewModel
            {
                Message = message,
                ReadOnly = basketPart.ReadOnly,
                CurrentPromoCode = currentPromoCode,
                CheckoutUrl = basketPart.CheckoutUrl,
                NumberFormat = this._webStoreServices.NumberFormat
            };

            this._webStoreServices.UsingClient(c => viewModel.Basket = c.BasketClient.GetBasket(this._webStoreServices.CurrentUserName, this._webStoreServices.BasketName));

            this.ModelState.Clear();

            return new ShapePartialResult(this, this._shapeFactory.EditorTemplate(TemplateName: "Basket/Basket", Model: viewModel));
        }
    }
}