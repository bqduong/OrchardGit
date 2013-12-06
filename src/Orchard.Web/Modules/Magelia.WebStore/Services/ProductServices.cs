using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.Product;
using Magelia.WebStore.Services.Contract.Parameters.Store;
using System;
using System.Linq;
using System.Web;

namespace Magelia.WebStore.Services
{
    public class ProductServices : IProductServices
    {
        private IWebStoreServices _webStoreServices;

        private ProductViewModel GetProductReference(ProductPart part)
        {
            ProductViewModel viewModel = new ProductViewModel();

            if (part.FromUrl && !String.IsNullOrEmpty(part.CatalogCodeUrlParameterKey) && !String.IsNullOrEmpty(part.SKUUrlParameterKey))
            {
                viewModel.RequestedCatalogCode = HttpContext.Current.Request.QueryString[part.CatalogCodeUrlParameterKey];
                viewModel.RequestedSKU = HttpContext.Current.Request.QueryString[part.SKUUrlParameterKey];
            }
            else if (!part.FromUrl)
            {
                viewModel.RequestedSKU = part.SKU;
                viewModel.RequestedCatalogCode = part.CatalogCode;
            }

            return viewModel;
        }

        public ProductServices(IWebStoreServices webStoreServices)
        {
            this._webStoreServices = webStoreServices;
        }

        public ProductViewModel GetModel(ProductPart part)
        {
            ProductViewModel viewModel = this.GetProductReference(part);

            if (!String.IsNullOrEmpty(viewModel.RequestedCatalogCode) && !String.IsNullOrEmpty(viewModel.RequestedSKU))
            {
                this._webStoreServices.UsingClient(
                    c =>
                    {
                        viewModel.ReferenceProduct = c.CatalogClient.Products.OfType<ReferenceProduct>()
                                                                             .Include(rp => rp.Brand)
                                                                             .Include(rp => rp.AttributeValues.Select(av => av.Files))
                                                                             .Include(rp => rp.AttributeValues.Select(av => av.Attribute))
                                                                             .Include(rp => rp.PriceWithLowerQuantity.TaxDetails)
                                                                             .Include(rp => rp.PriceWithLowerQuantity.DiscountDetails)
                                                                             .Include(rp => (rp as VariantProduct).VariableProduct.VariantProducts.Select(vp => vp.Brand))
                                                                             .Include(rp => (rp as VariantProduct).VariableProduct.VariantProducts.Select(vp => vp.AttributeValues.Select(av => av.Files)))
                                                                             .Include(rp => (rp as VariantProduct).VariableProduct.VariantProducts.Select(vp => vp.AttributeValues.Select(av => av.Attribute)))
                                                                             .Include(rp => (rp as VariantProduct).VariableProduct.VariantProducts.Select(vp => vp.PriceWithLowerQuantity.TaxDetails))
                                                                             .Include(rp => (rp as VariantProduct).VariableProduct.VariantProducts.Select(vp => vp.PriceWithLowerQuantity.DiscountDetails))
                                                                             .FirstOrDefault(rp => rp.Catalog.Code == viewModel.RequestedCatalogCode && rp.SKU == viewModel.RequestedSKU);

                        if (viewModel.ReferenceProduct != null)
                        {
                            viewModel.Inventories = c.StoreClient.GetInventory(new[] { viewModel.ReferenceProduct.ProductId }, new Location { CountryId = this._webStoreServices.CurrentCountryId, RegionId = this._webStoreServices.CurrentRegionId });
                        }
                    }
                );
            }

            return viewModel;
        }
    }
}