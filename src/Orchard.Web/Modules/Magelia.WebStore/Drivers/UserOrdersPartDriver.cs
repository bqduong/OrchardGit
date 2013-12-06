using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;

namespace Magelia.WebStore.Drivers
{
    public class UserOrdersPartDriver : ContentPartDriver<UserOrdersPart>
    {
        private Localizer _localizer;
        private IOrchardServices _orchardServices;
        private IWebStoreServices _webStoreServices;
        private IUserOrdersServices _userOrdersServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_UserOrders";
            }
        }

        protected override DriverResult Display(UserOrdersPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_UserOrders",
                () => shapeHelper.Parts_UserOrders(
                    PageSize: part.PageSize,
                    UserOrdersPartId: part.Id,
                    EnablePaging: part.EnablePaging,
                    EnableSorting: part.EnableSorting,
                    Orders: this._userOrdersServices.GetModel(part),
                    Authenticated: this._orchardServices.WorkContext.CurrentUser != null,
                    AvailableCurrencies: this._webStoreServices.StoreContext.AvailableCurrencies
                )
            );
        }

        protected override DriverResult Editor(UserOrdersPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_UserOrders_Edit",
                () => shapeHelper.EditorTemplate(
                    Model: part,
                    Prefix: this.Prefix,
                    TemplateName: "Parts/UserOrders"
                )
            );
        }

        protected override DriverResult Editor(UserOrdersPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);

            if (part.EnablePaging && !part.PageSize.HasValue)
            {
                updater.AddModelError("PageSizeRequired", this._localizer("Page size is required"));
            }

            return this.Editor(part, shapeHelper);
        }

        public UserOrdersPartDriver(IWebStoreServices webStoreServices, IUserOrdersServices userOrdersServices, IOrchardServices orchardServices)
        {
            this._orchardServices = orchardServices;
            this._localizer = NullLocalizer.Instance;
            this._webStoreServices = webStoreServices;
            this._userOrdersServices = userOrdersServices;
        }
    }
}