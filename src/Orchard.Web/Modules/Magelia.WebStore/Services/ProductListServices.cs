using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.CatalogHierarchy;
using Magelia.WebStore.Models.ViewModels.ProductList;
using Magelia.WebStore.Services.Contract.Parameters.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;

namespace Magelia.WebStore.Services
{
    public class ProductListServices : IProductListServices
    {
        private const String ProductListUserModelsStateCategory = "productlist";

        private Orchard.Logging.ILogger _logger;
        private IOrchardServices _orchardServices;
        private IWebStoreServices _webStoreServices;
        private IUserModelsStateServices _userModelsStateServices;
        private ICatalogHierarchyServices _catalogHierarchyServices;

        private Nullable<Int32> _page
        {
            get
            {
                Int32 page;
                if (Int32.TryParse(HttpContext.Current.Request.Url.GetAddedParameter(this.PageParameterKey), out page))
                {
                    return page;
                }
                return null;
            }
        }

        private Nullable<SortDirection> _sortDirection
        {
            get
            {
                SortDirection sortDirection;
                if (Enum.TryParse(HttpContext.Current.Request.Url.GetAddedParameter(this.SortDirectionParameterKey), out sortDirection))
                {
                    return sortDirection;
                }
                return null;
            }
        }

        private String _sortExpression
        {
            get
            {
                return HttpContext.Current.Request.Url.GetAddedParameter(this.SortExpressionParameterKey);
            }
        }

        private Nullable<Int32> _target
        {
            get
            {
                Int32 target;
                if (Int32.TryParse(HttpContext.Current.Request.Url.GetAddedParameter(this.TargetParameterKey), out target))
                {
                    return target;
                }
                return null;
            }
        }

        public String TargetParameterKey
        {
            get
            {
                return "plw_target";
            }
        }

        public String PageParameterKey
        {
            get
            {
                return "plw_page";
            }
        }

        public String SortDirectionParameterKey
        {
            get
            {
                return "plw_direction";
            }
        }

        public String SortExpressionParameterKey
        {
            get
            {
                return "plw_sort";
            }
        }

        private ProductListViewModel.ProductListState GetState(ProductListPart part)
        {
            return this._userModelsStateServices.GetFromCommerceContext<ProductListViewModel.ProductListState>(ProductListServices.ProductListUserModelsStateCategory, part.Id);
        }

        private HierarchyItemViewModel GetSelected(IEnumerable<HierarchyItemViewModel> items)
        {
            foreach (HierarchyItemViewModel item in items)
            {
                HierarchyItemViewModel subSelectedItem;

                if (item.Selected)
                {
                    return item;
                }
                else if ((subSelectedItem = this.GetSelected(item.Categories)) != null)
                {
                    return subSelectedItem;
                }
            }
            return null;
        }

        private Boolean Contains(IEnumerable<CategoryItemViewModel> categories, CategoryItemViewModel searchCategory)
        {
            foreach (CategoryItemViewModel category in categories)
            {
                if (category == searchCategory || this.Contains(category.Categories, searchCategory))
                {
                    return true;
                }
            }
            return false;
        }

        private CatalogItemViewModel GetCatalog(HierarchyViewModel hierarchy, CategoryItemViewModel category)
        {
            return hierarchy.FirstOrDefault(cata => this.Contains(cata.Categories, category));
        }

        private String GetPropertyName<T, P>(Expression<Func<T, P>> accessor)
        {
            return (accessor.Body as MemberExpression).Member.Name;
        }

        private void UpdateState(ProductListPart part, ProductListViewModel viewModel)
        {
            String previousCatalogCodeFilter = viewModel.State.CatalogCodeFilter ?? String.Empty;
            String previousCategoryCodeFilter = viewModel.State.CategoryCodeFilter ?? String.Empty;

            if (part.FromCatalogHierarchySelection)
            {
                HierarchyViewModel hierarchy;
                HierarchyItemViewModel selectedItem;
                CatalogHierarchyPart catalogHierarchyPart;
                if (
                    part.CatalogHierarchyId.HasValue &&
                    (catalogHierarchyPart = this._orchardServices.ContentManager.Get<CatalogHierarchyPart>(part.CatalogHierarchyId.Value, VersionOptions.Published)) != null &&
                    !catalogHierarchyPart.GenerateUrls &&
                    (hierarchy = this._catalogHierarchyServices.GetModel(catalogHierarchyPart)) != null &&
                    (selectedItem = this.GetSelected(hierarchy)) != null
                )
                {
                    if (selectedItem is CatalogItemViewModel)
                    {
                        CatalogItemViewModel catalog = (CatalogItemViewModel)selectedItem;
                        viewModel.State.CatalogCodeFilter = catalog.Catalog.Code;
                        viewModel.State.CategoryCodeFilter = null;
                    }
                    else if (selectedItem is CategoryItemViewModel)
                    {
                        CategoryItemViewModel category = (CategoryItemViewModel)selectedItem;
                        viewModel.State.CatalogCodeFilter = this.GetCatalog(hierarchy, category).Catalog.Code;
                        viewModel.State.CategoryCodeFilter = category.Category.Code;
                    }

                    viewModel.State.FromPath = this.GetPath(hierarchy, selectedItem);
                }
                else
                {
                    viewModel.State.FromPath = viewModel.State.CatalogCodeFilter = viewModel.State.CategoryCodeFilter = null;
                }
            }
            else
            {
                viewModel.State.CatalogCodeFilter = part.CatalogCodeFilter;
                viewModel.State.CategoryCodeFilter = part.CategoryCodeFilter;
            }
            if (this._target == part.Id)
            {
                viewModel.State.Page = this._page;
                viewModel.State.SortDirection = this._sortDirection;
                viewModel.State.SortExpression = this._sortExpression;
            }

            Boolean hasSortExpression = !String.IsNullOrEmpty(viewModel.State.SortExpression);
            Boolean hasCatalogFilter = !String.IsNullOrEmpty(viewModel.State.CatalogCodeFilter);
            Boolean hasCategoryFilter = !String.IsNullOrEmpty(viewModel.State.CategoryCodeFilter);

            if (part.EnableSorting)
            {
                if (!viewModel.State.SortDirection.HasValue)
                {
                    viewModel.State.SortDirection = SortDirection.Ascending;
                }
                if (!hasSortExpression)
                {
                    viewModel.State.SortExpression = hasCategoryFilter ? "custom" : this.GetPropertyName<ReferenceProduct, String>(rp => rp.Name);
                }
                else if ("custom".EqualsOrdinalIgnoreCase(viewModel.State.SortExpression) && !hasCategoryFilter)
                {
                    viewModel.State.SortExpression = this.GetPropertyName<ReferenceProduct, String>(rp => rp.Name);
                }
            }
            else
            {
                viewModel.State.SortDirection = SortDirection.Ascending;
                viewModel.State.SortExpression = String.IsNullOrEmpty(viewModel.State.CategoryCodeFilter) ? this.GetPropertyName<BaseProduct, String>(bp => bp.Name) : "custom";
            }
            if (
                (!previousCatalogCodeFilter.EqualsOrdinalIgnoreCase(viewModel.State.CatalogCodeFilter ?? String.Empty) && !this._page.HasValue) ||
                (!previousCategoryCodeFilter.EqualsOrdinalIgnoreCase(viewModel.State.CategoryCodeFilter ?? String.Empty) && !this._page.HasValue) ||
                !part.EnablePaging ||
                viewModel.State.Page < 1 ||
                !viewModel.State.Page.HasValue
            )
            {
                viewModel.State.Page = 1;
            }
        }

        private String GetPath(IEnumerable<HierarchyItemViewModel> items, HierarchyItemViewModel selectedItem)
        {
            HierarchyItemViewModel parentItem;
            if (items.Contains(selectedItem))
            {
                return this.GetCode(selectedItem);
            }
            else if (selectedItem is CategoryItemViewModel && (parentItem = items.FirstOrDefault(i => this.Contains(i.Categories, selectedItem as CategoryItemViewModel))) != null)
            {
                return String.Concat(this.GetCode(parentItem), this._catalogHierarchyServices.PathSeparator, this.GetPath(parentItem.Categories, selectedItem));
            }
            return null;
        }

        private String GetCode(HierarchyItemViewModel item)
        {
            if (item is CatalogItemViewModel)
            {
                return ((CatalogItemViewModel)item).Catalog.Code;
            }
            else if (item is CategoryItemViewModel)
            {
                return ((CategoryItemViewModel)item).Category.Code;
            }
            return null;
        }

        private IQueryable<ReferenceProduct> OrderAndPagineProducts<TTarget>(IQueryable<ReferenceProduct> products, SortDirection sortDirection, Int32 skip, Int32 take, Expression<Func<ReferenceProduct, TTarget>> orderExpression)
        {
            return (sortDirection == SortDirection.Ascending ? products.OrderBy(orderExpression) : products.OrderByDescending(orderExpression)).Skip(skip).Take(take);
        }

        private Expression GetMemberExpression(ParameterExpression parameter, String sortExpression)
        {
            if (!String.IsNullOrEmpty(sortExpression))
            {
                Expression expression = parameter;
                Type baseProductType = typeof(BaseProduct);
                List<String> members = sortExpression.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).ToList();
                IEnumerable<PropertyInfo> properties = typeof(ReferenceProduct).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                for (Int32 i = 0; i < members.Count; i++)
                {
                    String member = members[i];
                    PropertyInfo property = properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase(member));

                    if (property == null)
                    {
                        return null;
                    }

                    if (i == 0 && property.DeclaringType == baseProductType)
                    {
                        Type variantProductType = typeof(VariantProduct);
                        expression = Expression.Condition(
                            Expression.TypeIs(parameter, variantProductType),
                            Expression.TypeAs(Expression.Property(Expression.TypeAs(parameter, variantProductType), this.GetPropertyName<VariantProduct, VariableProduct>(vp => vp.VariableProduct)), baseProductType),
                            Expression.TypeAs(parameter, baseProductType)
                        );
                    }

                    expression = Expression.Property(expression, property);
                    properties = property.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                }

                return expression;
            }

            return null;
        }

        private IQueryable<ReferenceProduct> CustomOrder(IQueryable<ReferenceProduct> products, ProductListViewModel viewModel, Int32 skip, Int32 take)
        {
            if (!String.IsNullOrEmpty(viewModel.State.CatalogCodeFilter))
            {
                products = this.OrderAndPagineProducts(
                    products,
                    viewModel.State.SortDirection.Value,
                    skip,
                    take,
                    rp => rp is VariantProduct ?
                            (rp as VariantProduct).VariableProduct.ProductCategories.FirstOrDefault(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter && pc.Category.Catalog.Code == viewModel.State.CatalogCodeFilter).Order :
                            rp.ProductCategories.FirstOrDefault(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter && pc.Category.Catalog.Code == viewModel.State.CatalogCodeFilter).Order
                );
            }
            else
            {
                products = this.OrderAndPagineProducts(
                    products,
                    viewModel.State.SortDirection.Value,
                    skip,
                    take,
                    rp => rp is VariantProduct ?
                            (rp as VariantProduct).VariableProduct.ProductCategories.FirstOrDefault(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter).Order :
                            rp.ProductCategories.FirstOrDefault(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter).Order
                );
            }
            return products;
        }

        private IQueryable<ReferenceProduct> OrderByMember(IQueryable<ReferenceProduct> products, SortDirection sortDirection, Int32 skip, Int32 take, Expression memberExpression, ParameterExpression parameter)
        {
            return (IQueryable<ReferenceProduct>)new Func<IQueryable<ReferenceProduct>, SortDirection, Int32, Int32, Expression<Func<ReferenceProduct, Object>>, IQueryable<ReferenceProduct>>(this.OrderAndPagineProducts<Object>).Method.GetGenericMethodDefinition().MakeGenericMethod(memberExpression.Type).Invoke(this, new Object[] { products, sortDirection, skip, take, Expression.Lambda(memberExpression, parameter) });
        }

        private IQueryable<ReferenceProduct> OrderByAttribute(IQueryable<ReferenceProduct> products, SortDirection sortDirection, Int32 skip, Int32 take, Magelia.WebStore.Client.Attribute attribute)
        {
            switch (attribute.AttributeTypeCode.ToLowerInvariant())
            {
                case "boolean":
                    products = this.OrderAndPagineProducts(
                        products,
                        sortDirection,
                        skip,
                        take,
                        rp => rp.AttributeValues.Where(av => av.Attribute.Code == attribute.Code).Select(av => av.BooleanValue).FirstOrDefault()
                    );
                    break;
                case "integer":
                    products = this.OrderAndPagineProducts(
                        products,
                        sortDirection,
                        skip,
                        take,
                        rp => rp.AttributeValues.Where(av => av.Attribute.Code == attribute.Code).Select(av => av.IntValue).FirstOrDefault()
                    );
                    break;
                case "decimal":
                    products = this.OrderAndPagineProducts(
                        products,
                        sortDirection,
                        skip,
                        take,
                        rp => rp.AttributeValues.Where(av => av.Attribute.Code == attribute.Code).Select(av => av.DecimalValue).FirstOrDefault()
                    );
                    break;
                case "datetime":
                    products = this.OrderAndPagineProducts(
                        products,
                        sortDirection,
                        skip,
                        take,
                        rp => rp.AttributeValues.Where(av => av.Attribute.Code == attribute.Code).Select(av => av.DateTimeValue).FirstOrDefault()
                    );
                    break;
                case "list":
                    products = this.OrderAndPagineProducts(
                        products,
                        sortDirection,
                        skip,
                        take,
                        rp => rp.AttributeValues.Where(av => av.Attribute.Code == attribute.Code).Select(av => av.ListValue).FirstOrDefault()
                    );
                    break;
                case "string":
                    products = this.OrderAndPagineProducts(
                        products,
                        sortDirection,
                        skip,
                        take,
                        rp => rp.AttributeValues.Where(av => av.Attribute.Code == attribute.Code).Select(av => av.StringValue).FirstOrDefault()
                    );
                    break;
                default:
                    products = this.OrderAndPagineProducts(
                        products,
                        sortDirection,
                        skip,
                        take,
                        rp => rp.AttributeValues.Where(av => av.Attribute.Code == attribute.Code).Select(av => av.Files.Count()).FirstOrDefault()
                    );
                    break;
            }
            return products;
        }

        private void LoadProducts(ProductListPart part, ProductListViewModel viewModel)
        {
            viewModel.Clear();

            this._webStoreServices.UsingClient(
                c =>
                {
                    Expression memberExpression;
                    Magelia.WebStore.Client.Attribute attribute;
                    Boolean hasCategoryFilter, hasCatalogFilter;

                    ParameterExpression parameter = Expression.Parameter(typeof(ReferenceProduct));

                    Int32 skip = (part.PageSize ?? 0) * ((viewModel.State.Page ?? 1) - 1);
                    Int32 take = part.EnablePaging && part.PageSize.HasValue ? part.PageSize.Value : Int32.MaxValue;

                    IQueryable<ReferenceProduct> products = c.CatalogClient.Products.OfType<ReferenceProduct>()
                                                                                    .Include(rp => rp.Brand)
                                                                                    .Include(rp => rp.Catalog)
                                                                                    .Include(rp => rp.AttributeValues.Select(av => av.Files))
                                                                                    .Include(rp => rp.PriceWithLowerQuantity.TaxDetails)
                                                                                    .Include(rp => rp.PriceWithLowerQuantity.DiscountDetails)
                                                                                    .Include(rp => (rp as VariantProduct).VariableProduct)
                                                                                    .Where(rp => !(rp is VariantProduct) || ((rp as VariantProduct).IsDefault));

                    if (hasCatalogFilter = !String.IsNullOrEmpty(viewModel.State.CatalogCodeFilter))
                    {
                        products = products.Where(p => p.Catalog.Code == viewModel.State.CatalogCodeFilter);
                    }
                    if (hasCategoryFilter = !String.IsNullOrEmpty(viewModel.State.CategoryCodeFilter))
                    {
                        products = products.Where(p => p is VariantProduct ? (p as VariantProduct).VariableProduct.ProductCategories.Any(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter) : p.ProductCategories.Any(pc => pc.Category.Code == viewModel.State.CategoryCodeFilter));
                    }

                    viewModel.State.PageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(products.Count()) / Convert.ToDecimal(take)));

                    viewModel.DerivedAttributes = products.SelectMany(p => p.AttributeValues)
                                                   .Select(av => av.Attribute)
                                                   .Where(a => a.ShowInProductList)
                                                   .GroupBy(a => a.Code)
                                                   .OrderBy(g => g.Min(a => a.Order))
                                                   .ToDictionary(g => g.Key, g => g.AsEnumerable());

                    if ("custom".EqualsOrdinalIgnoreCase(viewModel.State.SortExpression))
                    {
                        products = this.CustomOrder(products, viewModel, skip, take);
                    }
                    else if ((memberExpression = this.GetMemberExpression(parameter, viewModel.State.SortExpression)) != null)
                    {
                        products = this.OrderByMember(products, viewModel.State.SortDirection.Value, skip, take, memberExpression, parameter);
                    }
                    else if ((attribute = viewModel.DerivedAttributes.Where(a => a.Key.EqualsOrdinalIgnoreCase(viewModel.State.SortExpression)).SelectMany(kvp => kvp.Value).FirstOrDefault()) != null)
                    {
                        products = this.OrderByAttribute(products, viewModel.State.SortDirection.Value, skip, take, attribute);
                    }
                    else
                    {
                        this._logger.Warning("SortExpression could not by applied : {0}", viewModel.State.SortExpression);
                        products = this.OrderAndPagineProducts(products, viewModel.State.SortDirection.Value, skip, take, rp => rp is VariantProduct ? (rp as VariantProduct).VariableProduct.Name : rp.Name);
                    }

                    viewModel.AddRange(products);
                }
            );
        }

        private void SetStock(ProductListPart part, ProductListViewModel viewModel)
        {
            if (part.DisplayProductsAvailability)
            {
                this._webStoreServices.UsingClient(
                    c => viewModel.Inventories = c.StoreClient.GetInventory(
                        viewModel.Select(p => p.ProductId).ToList(), 
                        new Location { 
                            CountryId = this._webStoreServices.CurrentCountryId, 
                            RegionId = this._webStoreServices.CurrentRegionId 
                        }
                    )
                );
            }
        }

        public ProductListServices(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IUserModelsStateServices userModelsStateServices, ICatalogHierarchyServices catalogHierarchyServices)
        {
            this._logger = NullLogger.Instance;
            this._orchardServices = orchardServices;
            this._webStoreServices = webStoreServices;
            this._userModelsStateServices = userModelsStateServices;
            this._catalogHierarchyServices = catalogHierarchyServices;
        }

        public ProductListViewModel GetModel(ProductListPart part)
        {
            ProductListViewModel viewModel = new ProductListViewModel();
            viewModel.State = this.GetState(part);
            this.UpdateState(part, viewModel);
            this.LoadProducts(part, viewModel);
            this.SetStock(part, viewModel);
            return viewModel;
        }
    }
}