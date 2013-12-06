using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;

namespace Magelia.WebStore.Drivers
{
    public class ProductPartDriver : ContentPartDriver<ProductPart>
    {
        private Localizer _localizer;
        private IProductServices _productServices;
        private IWebStoreServices _webStoreServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_Product";
            }
        }

        protected override DriverResult Display(ProductPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_Product",
                () => shapeHelper.Parts_Product(
                        FromUrl: part.FromUrl,
                        AllowAddToBasket: part.AllowAddToBasket,
                        SKUUrlParameterKey: part.SKUUrlParameterKey,
                        Target: this._productServices.GetModel(part),
                        NumberFormat: this._webStoreServices.NumberFormat,
                        CatalogCodeUrlParameterKey: part.CatalogCodeUrlParameterKey
                    )
            );
        }

        protected override DriverResult Editor(ProductPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_Product_Edit",
                () => shapeHelper.EditorTemplate(
                    Model: part,
                    Prefix: this.Prefix,
                    TemplateName: "Parts/Product"
                )
            );
        }

        protected override DriverResult Editor(ProductPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);

            if (part.FromUrl)
            {
                if (String.IsNullOrEmpty(part.CatalogCodeUrlParameterKey))
                {
                    updater.AddModelError("CatalogCodeUrlParameterKeyRequired", this._localizer("Catalog code url parameter key is required"));
                }
                if (String.IsNullOrEmpty(part.SKUUrlParameterKey))
                {
                    updater.AddModelError("SKUUrlParameterKeyRequired", this._localizer("SKU url parameter key is required"));
                }
            }
            else
            {
                if (String.IsNullOrEmpty(part.CatalogCode))
                {
                    updater.AddModelError("CatalogCodeRequired", this._localizer("Catalog code is required"));
                }
                if (String.IsNullOrEmpty(part.SKU))
                {
                    updater.AddModelError("SKUUrlParameterKeyRequired", this._localizer("SKU is required"));
                }
            }
            return this.Editor(part, shapeHelper);
        }

        public ProductPartDriver(IWebStoreServices webStoreServices, IProductServices productService)
        {
            this._productServices = productService;
            this._localizer = NullLocalizer.Instance;
            this._webStoreServices = webStoreServices;
        }
    }
}