using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.ProductList;
using Orchard;
using System;

namespace Magelia.WebStore.Contracts
{
    public interface IProductListServices : IDependency
    {
        String PageParameterKey { get; }
        String TargetParameterKey { get; }
        String SortDirectionParameterKey { get; }
        String SortExpressionParameterKey { get; }
        ProductListViewModel GetModel(ProductListPart part);
    }
}