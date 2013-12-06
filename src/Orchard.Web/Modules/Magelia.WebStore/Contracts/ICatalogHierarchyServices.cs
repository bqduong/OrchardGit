using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.CatalogHierarchy;
using Orchard;
using System;

namespace Magelia.WebStore.Contracts
{
    public interface ICatalogHierarchyServices : IDependency
    {
        Char PathSeparator { get; }
        String PathParameterKey { get; }
        String TargetParameterKey { get; }
        String ActionParameterKey { get; }
        String SelectParameterValue { get; }
        String ExpandParameterValue { get; }
        String CollapseParameterValue { get; }
        HierarchyViewModel GetModel(CatalogHierarchyPart part);
    }
}
