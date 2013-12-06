using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.MVC;
using Magelia.WebStore.Services.Contract.Parameters.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Magelia.WebStore.Controllers
{
    public class ContextController : Controller
    {
        private IOrchardServices _orchardServices;
        private IWebStoreServices _webStoreServices;

        private RedirectResult RedirectOnReferer()
        {
            return this.Redirect(this.Request.UrlReferrer.RemoveAddedParameters().ToString());
        }

        private void UpdateBasket()
        {
            this._webStoreServices.UsingClient(c => c.BasketClient.UpdateBasket(this._webStoreServices.CurrentUserName, this._webStoreServices.BasketName, this._webStoreServices.CurrentCurrencyId, CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).LCID, new Location { CountryId = this._webStoreServices.CurrentCountryId, RegionId = this._webStoreServices.CurrentRegionId }));
        }

        public ContextController(IWebStoreServices webStoreServices, IOrchardServices orchardServices)
        {
            this._orchardServices = orchardServices;
            this._webStoreServices = webStoreServices;
        }

        [HttpGet]
        [NoCache]
        public ActionResult Magelia()
        {
            this.Response.ContentType = "text/javascript";
            return this.View();
        }

        [HttpGet]
        [NoCache]
        public JsonResult GetRegions(Int32 countryId)
        {
            Object result = null;

            this._webStoreServices.UsingClient(
                c => result = c.StoreClient.GetRegions(countryId, this._webStoreServices.CurrentCultureId)
                                           .Select(r => new { regionId = r.RegionId, name = r.Name })
            );

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [NoCache]
        public ActionResult LocationPicker(Int32 locationPickerPartId)
        {
            return new ShapePartialResult(this, this._orchardServices.ContentManager.BuildDisplay(this._orchardServices.ContentManager.Get(locationPickerPartId, VersionOptions.Published)));
        }

        [HttpPost]
        public RedirectResult UpdateLocation(Int32 countryId, Nullable<Guid> regionId)
        {
            this._webStoreServices.CurrentRegionId = regionId;
            this._webStoreServices.CurrentCountryId = countryId;

            this.UpdateBasket();

            return this.RedirectOnReferer();
        }

        [HttpPost]
        public RedirectResult UpdateCurrency(Int32 currencyId)
        {
            this._webStoreServices.CurrentCurrencyId = currencyId;

            this.UpdateBasket();

            return this.RedirectOnReferer();
        }
    }
}