using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;

namespace Magelia.WebStore.Drivers
{
    public class CurrencyPickerPartDriver : ContentPartDriver<CurrencyPickerPart>
    {
        private IWebStoreServices _webStoreServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_CurrencyPicker";
            }
        }

        protected override DriverResult Display(CurrencyPickerPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_CurrencyPicker",
                () => shapeHelper.Parts_CurrencyPicker(
                    CurrencyPickerPartId: part.Id,
                    CurrentCurrencyId: this._webStoreServices.CurrentCurrencyId,
                    Currencies: this._webStoreServices.StoreContext.AvailableCurrencies
                )
            );
        }

        protected override DriverResult Editor(CurrencyPickerPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_CurrencyPicker_Edit",
                () => shapeHelper.EditorTemplate(
                    Model: part,
                    Prefix: this.Prefix,
                    TemplateName: "Parts/CurrencyPicker"
                )
            );
        }

        protected override DriverResult Editor(CurrencyPickerPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);
            return this.Editor(part, shapeHelper);
        }

        public CurrencyPickerPartDriver(IWebStoreServices webStoreServices)
        {
            this._webStoreServices = webStoreServices;
        }
    }
}