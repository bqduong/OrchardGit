using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Events;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.User;
using Magelia.WebStore.MVC;
using Magelia.WebStore.Services.Contract.Data.Store;
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
using Customer = Magelia.WebStore.Services.Contract.Data.Customer;

namespace Magelia.WebStore.Controllers
{
    public class UserController : Controller
    {
        private Localizer _localizer;
        private dynamic _shapeFactory;
        private IOrchardServices _orchardServices;
        private IWebStoreServices _webStoreServices;
        private IUserOrdersServices _userOrdersServices;
        private IEnumerable<IAddressEventHandler> _addressEventHandlers;

        private AddressViewModel Initialize(AddressViewModel viewModel)
        {
            if (viewModel != null)
            {
                viewModel.Countries = this._webStoreServices.StoreContext.AvailableCountries.Select(ac => new SelectListItem { Text = ac.Name, Value = ac.CountryId.ToString() }).ToList();

                if (viewModel.CountryId.HasValue)
                {
                    this._webStoreServices.UsingClient(
                        c => viewModel.Regions = c.StoreClient.GetRegions(viewModel.CountryId.Value, this._webStoreServices.CurrentCultureId)
                                                                                             .Select(r => new SelectListItem { Text = r.Name, Value = r.RegionId.ToString() })
                                                                                             .ToList()
                    );
                }
            }
            return viewModel;
        }

        private Customer.Address GetAddress(AddressViewModel viewModel)
        {
            return new Customer.Address
            {
                AddressId = viewModel.AddressId.HasValue ? viewModel.AddressId.Value : Guid.NewGuid(),
                City = viewModel.City,
                Company = viewModel.Company,
                CountryId = viewModel.CountryId.Value,
                DigiCode = viewModel.DigiCode,
                Email = viewModel.Email,
                FaxNumber = viewModel.FaxNumber,
                FirstName = viewModel.FirstName,
                Floor = viewModel.Floor,
                LastName = viewModel.LastName,
                Line1 = viewModel.Line1,
                Line2 = viewModel.Line2,
                Line3 = viewModel.Line3,
                MiddleName = viewModel.MiddleName,
                MobileNumber = viewModel.MobileNumber,
                Name = viewModel.Name,
                PhoneNumber = viewModel.PhoneNumber,
                RegionId = viewModel.RegionId,
                ZipCode = viewModel.ZipCode
            };
        }

        private OrderAddressViewModel GetAddress(OrderAddress address, IEnumerable<Magelia.WebStore.Services.Contract.Data.Store.Country> countries, WebStoreContext client)
        {
            if (address != null)
            {
                IEnumerable<Magelia.WebStore.Services.Contract.Data.Store.Region> regions = address.RegionId.HasValue ?
                                                                                                client.StoreClient.GetRegions(address.CountryId, this._webStoreServices.CurrentCultureId) :
                                                                                                Enumerable.Empty<Magelia.WebStore.Services.Contract.Data.Store.Region>();

                return new OrderAddressViewModel
                {
                    City = address.CityName,
                    Company = address.CompanyName,
                    CountryName = countries.Where(c => c.CountryId == address.CountryId).Select(c => c.Name).FirstOrDefault(),
                    DigiCode = address.DigiCode,
                    Email = address.EmailAddress,
                    FaxNumber = address.FaxNumber,
                    FirstName = address.FirstName,
                    Floor = address.FloorNumber,
                    LastName = address.LastName,
                    Line1 = address.Address1,
                    Line2 = address.Address2,
                    Line3 = address.Address3,
                    MiddleName = address.MiddleName,
                    MobileNumber = address.MobileNumber,
                    Name = address.Name,
                    PhoneNumber = address.PhoneNumber,
                    RegionName = regions.Where(r => r.RegionId == address.RegionId).Select(r => r.Name).FirstOrDefault(),
                    ZipCode = address.ZipCode
                };
            }
            return null;
        }

        private ShapePartialResult BuildAddressShape(AddressViewModel viewModel)
        {
            return new ShapePartialResult(this, this._shapeFactory.EditorTemplate(TemplateName: "User/Address", Model: this.Initialize(viewModel)));
        }

        public UserController(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IUserOrdersServices userOrdersServices, IEnumerable<IAddressEventHandler> addressEventHandlers, IShapeFactory shapeFactory)
        {
            this._shapeFactory = shapeFactory;
            this._orchardServices = orchardServices;
            this._localizer = NullLocalizer.Instance;
            this._webStoreServices = webStoreServices;
            this._userOrdersServices = userOrdersServices;
            this._addressEventHandlers = addressEventHandlers;
        }

        [HttpGet]
        [NoCache]
        [Authorize]
        public JsonResult GetAddresses()
        {
            Object result = null;

            this._webStoreServices.UsingClient(
                c => result = c.CustomerClient.GetAddresses(this._orchardServices.WorkContext.CurrentUser.UserName, this._webStoreServices.CurrentCultureId)
                                              .Select(a => new { name = a.Name, addressId = a.AddressId })
                                              .ToList()
            );

            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [NoCache]
        [Authorize]
        public ActionResult NewAddress()
        {
            return this.BuildAddressShape(
                new AddressViewModel
                {
                    Named = true,
                    RegionId = this._webStoreServices.CurrentRegionId,
                    CountryId = this._webStoreServices.CurrentCountryId
                }
            );
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveAddress(AddressViewModel viewModel)
        {
            if (this.ModelState.IsValid)
            {
                Boolean success = true;
                this._webStoreServices.UsingClient(
                    c =>
                    {
                        if (viewModel.RegionId.HasValue || (!viewModel.RegionId.HasValue && !c.StoreClient.GetRegions(viewModel.CountryId.Value, this._webStoreServices.CurrentCultureId).Any()))
                        {
                            List<Customer.Address> addresses = c.CustomerClient.GetAddresses(this._orchardServices.WorkContext.CurrentUser.UserName, this._webStoreServices.CurrentCultureId).ToList();
                            if (addresses.Any(a => a.Name.Equals(viewModel.Name, StringComparison.InvariantCulture) && a.AddressId != viewModel.AddressId))
                            {
                                success = false;
                                this.ModelState.AddModelError("DuplicatedName", this._localizer("Address name already used").ToString());
                            }
                            else
                            {
                                Customer.Address newAddress = this.GetAddress(viewModel);
                                Customer.Address previousAddress = addresses.FirstOrDefault(a => a.AddressId == viewModel.AddressId);

                                addresses.Add(newAddress);
                                addresses.Remove(previousAddress);

                                c.CustomerClient.UpdateAddresses(this._orchardServices.WorkContext.CurrentUser.UserName, addresses);

                                if (previousAddress == null)
                                {
                                    this._addressEventHandlers.Trigger(h => h.Created(newAddress));
                                }
                                else
                                {
                                    this._addressEventHandlers.Trigger(h => h.Updated(newAddress));
                                }
                            }
                        }
                        else
                        {
                            success = false;
                            this.ModelState.AddModelError("RegionRequired", this._localizer("Region is required").ToString());
                        }
                    }
                );
                if (success)
                {
                    return this.Json(new { name = viewModel.Name, shippingAddressIsDifferent = viewModel.ShippingAddressIsDifferent });
                }
            }
            return this.BuildAddressShape(viewModel);
        }

        [HttpGet]
        [NoCache]
        [Authorize]
        public PartialViewResult GetAddress(Guid addressId, Nullable<Boolean> promptShippingAddressIsDifferent, Nullable<Boolean> shippingAddressIsDifferent)
        {
            AddressViewModel viewModel = null;
            this._webStoreServices.UsingClient(
                c =>
                {
                    Customer.Address address = c.CustomerClient.GetAddresses(this._orchardServices.WorkContext.CurrentUser.UserName, this._webStoreServices.CurrentCultureId)
                                                               .FirstOrDefault(a => a.AddressId == addressId);

                    if (address != null)
                    {
                        viewModel = new AddressViewModel
                        {
                            PromptShippingAddressIsDifferent = promptShippingAddressIsDifferent.HasValue && promptShippingAddressIsDifferent.Value,
                            ShippingAddressIsDifferent = shippingAddressIsDifferent.HasValue && shippingAddressIsDifferent.Value,
                            Named = true,
                            AddressId = address.AddressId,
                            City = address.City,
                            Company = address.Company,
                            CountryId = address.CountryId,
                            DigiCode = address.DigiCode,
                            Email = address.Email,
                            FaxNumber = address.FaxNumber,
                            FirstName = address.FirstName,
                            Floor = address.Floor,
                            LastName = address.LastName,
                            Line1 = address.Line1,
                            Line2 = address.Line2,
                            Line3 = address.Line3,
                            MiddleName = address.MiddleName,
                            MobileNumber = address.MobileNumber,
                            Name = address.Name,
                            PhoneNumber = address.PhoneNumber,
                            RegionId = address.RegionId,
                            ZipCode = address.ZipCode
                        };
                    }
                }
            );

            return this.BuildAddressShape(viewModel);
        }

        [Authorize]
        [HttpDelete]
        public void DeleteAddress(Guid addressId)
        {
            this._webStoreServices.UsingClient(c =>
                {
                    List<Customer.Address> addresses = c.CustomerClient.GetAddresses(this._orchardServices.WorkContext.CurrentUser.UserName, this._webStoreServices.CurrentCultureId).ToList();
                    Customer.Address addressToRemove = addresses.FirstOrDefault(a => a.AddressId == addressId);

                    if (addressToRemove != null)
                    {
                        addresses.Remove(addressToRemove);
                        c.CustomerClient.UpdateAddresses(this._orchardServices.WorkContext.CurrentUser.UserName, addresses);

                        this._addressEventHandlers.Trigger(h => h.Removed(addressToRemove));
                    }
                }
            );
        }

        [HttpGet]
        [NoCache]
        [Authorize]
        public ActionResult GetOrders(Int32 userOrdersPartId, Nullable<OrderSortDirection> sortDirection, Nullable<OrderSortExpression> sortExpression, Nullable<Int32> page)
        {
            UserOrdersPart userOrdersPart = this._orchardServices.ContentManager.Get<UserOrdersPart>(userOrdersPartId, VersionOptions.Published);

            this._userOrdersServices.UpdateState(userOrdersPart, sortDirection, sortExpression, page);

            return new ShapePartialResult(this, this._orchardServices.ContentManager.BuildDisplay(userOrdersPart));
        }

        [HttpGet]
        [NoCache]
        [Authorize]
        public ActionResult GetOrder(Int32 userOrdersPartId, Guid orderId)
        {
            OrderViewModel viewModel = new OrderViewModel
            {
                UserOrdersPartId = userOrdersPartId,
                NumberFormat = (NumberFormatInfo)CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).NumberFormat.Clone()
            };

            this._webStoreServices.UsingClient(
                c =>
                {
                    IEnumerable<Magelia.WebStore.Services.Contract.Data.Store.Country> countries = c.StoreClient.GetAllCountries(this._webStoreServices.CurrentCultureId);
                    viewModel.Order = c.OrderClient.GetOrder(this._webStoreServices.CurrentUserName, orderId);
                    viewModel.BillingAddress = this.GetAddress(viewModel.Order.BillingAddress, countries, c);
                    viewModel.ShippingAddress = this.GetAddress(viewModel.Order.ShippingAddress, countries, c);
                }
            );

            viewModel.NumberFormat.CurrencySymbol = this._webStoreServices.StoreContext.AvailableCurrencies.Where(ac => ac.CurrencyId == viewModel.Order.CurrencyId)
                                                                                                           .Select(ac => ac.Symbol)
                                                                                                           .FirstOrDefault();

            return new ShapePartialResult(this, this._shapeFactory.DisplayTemplate(TemplateName: "User/Order", Model: viewModel));
        }
    }
}