using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.ProductList;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Magelia.WebStore.Drivers
{
    public class ProductListPartDriver : ContentPartDriver<ProductListPart>
    {
        public class ProductListTools
        {
            private ProductListViewModel _products;
            private ProductListPart _productListPart;
            private IProductListServices _productListServices;
            private ICatalogHierarchyServices _catalogHierarchyServices;

            public ProductListTools(ProductListPart productListPart, ProductListViewModel products, IProductListServices productListServices, ICatalogHierarchyServices catalogHierarchyServices)
            {
                this._products = products;
                this._productListPart = productListPart;
                this._productListServices = productListServices;
                this._catalogHierarchyServices = catalogHierarchyServices;
            }

            public String GenerateUrl(Uri uri, Nullable<Int32> page, SortDirection sortDirection, String sortExpression)
            {
                NameValueCollection parameters = new NameValueCollection 
                {
                    {this._productListServices.TargetParameterKey, this._productListPart.Id.ToString()},
                    {this._productListServices.PageParameterKey, (page ?? 1).ToString()},
                    {this._productListServices.SortExpressionParameterKey, sortExpression},
                    {this._productListServices.SortDirectionParameterKey, Enum.GetName(typeof(SortDirection), sortDirection ) }
                };

                if (this._productListPart.CatalogHierarchyId.HasValue && !String.IsNullOrEmpty(this._products.State.FromPath))
                {
                    parameters.Add(this._catalogHierarchyServices.TargetParameterKey, this._productListPart.CatalogHierarchyId.ToString());
                    parameters.Add(this._catalogHierarchyServices.ActionParameterKey, this._catalogHierarchyServices.SelectParameterValue);
                    parameters.Add(this._catalogHierarchyServices.PathParameterKey, this._products.State.FromPath);
                }

                return uri.AddParameters(parameters).ToString();
            }
        }

        private Localizer _localizer;
        private IOrchardServices _orchardServices;
        private IWebStoreServices _webStoreServices;
        private IProductListServices _productListServices;
        private ICatalogHierarchyServices _catalogHierarchyServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_ProductList";
            }
        }

        protected override DriverResult Display(ProductListPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_ProductList",
                () =>
                {
                    ProductListViewModel model = this._productListServices.GetModel(part);
                    return shapeHelper.Parts_ProductList(
                         Products: model,
                         ProductListPartId: part.Id,
                         EnablePaging: part.EnablePaging,
                         EnableSorting: part.EnableSorting,
                         AllowAddToBasket: part.AllowAddToBasket,
                         DerivedAttributes: model.DerivedAttributes,
                         FromCatalogHierarchyId: part.CatalogHierarchyId,
                         ViewDetailUrlPattern: part.ViewDetailUrlPattern,
                         NumberFormat: this._webStoreServices.NumberFormat,
                         DisplayProductsAvailability: part.DisplayProductsAvailability,
                         DisplayViewDetail: part.DisplayViewDetail && !String.IsNullOrEmpty(part.ViewDetailUrlPattern),
                         Tools: new ProductListTools(part, model, this._productListServices, this._catalogHierarchyServices)
                    );
                }
            );
        }

        protected override DriverResult Editor(ProductListPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_ProductList_Edit",
                () => shapeHelper.EditorTemplate(
                    Prefix: this.Prefix,
                    TemplateName: "Parts/ProductList",
                    Model: new ProductListEditViewModel
                    {
                        Part = part,
                        CatalogHierarchies = this._orchardServices.ContentManager.Query<CatalogHierarchyPart>(VersionOptions.Latest)
                                                                                 .List()
                                                                                 .Where(chp => !chp.GenerateUrls)
                                                                                 .Select(
                                                                                     chp =>
                                                                                     {
                                                                                         ContentItemMetadata metadata = this._orchardServices.ContentManager.GetItemMetadata(chp);
                                                                                         return new SelectListItem { Text = String.IsNullOrEmpty(metadata.DisplayText) ? chp.TypeDefinition.DisplayName : metadata.DisplayText, Value = chp.Id.ToString(), Selected = chp.Id == part.CatalogHierarchyId };
                                                                                     }
                                                                                 )
                                                                                 .OrderBy(sli => sli.Text)
                                                                                 .ToList()
                    }
                )
            );
        }

        protected override DriverResult Editor(ProductListPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(new ProductListEditViewModel { Part = part }, this.Prefix, null, null);

            if (part.EnablePaging && !part.PageSize.HasValue)
            {
                updater.AddModelError("PageSizeRequired", this._localizer("Page size is required"));
            }
            if (part.FromCatalogHierarchySelection && !part.CatalogHierarchyId.HasValue)
            {
                updater.AddModelError("CatalogHierarchyIdRequired", this._localizer("Magelia WebStore Catalogs and Categories component is required"));
            }
            if (part.DisplayViewDetail && String.IsNullOrEmpty(part.ViewDetailUrlPattern))
            {
                updater.AddModelError("ViewDetailUrlPatternRequired", this._localizer("Url pattern is required"));
            }

            return this.Editor(part, shapeHelper);
        }

        public ProductListPartDriver(IWebStoreServices webStoreServices, IProductListServices productListServices, ICatalogHierarchyServices catalogHierarchyServices, IOrchardServices orchardServices)
        {
            this._orchardServices = orchardServices;
            this._localizer = NullLocalizer.Instance;
            this._webStoreServices = webStoreServices;
            this._productListServices = productListServices;
            this._catalogHierarchyServices = catalogHierarchyServices;
        }
    }
}