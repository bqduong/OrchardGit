using Magelia.WebStore.Contracts;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Models.ViewModels.CatalogHierarchy;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Magelia.WebStore.Drivers
{
    public class CatalogHierarchyPartDriver : ContentPartDriver<CatalogHierarchyPart>
    {
        public class CatalogHierarchyTools
        {
            private Lazy<Uri> _targetPath;
            private IOrchardServices _orchardServices;
            private CatalogHierarchyPart _catalogHieratchyPart;
            private ICatalogHierarchyServices _catalogHierarchyServices;

            public Uri TargetPath
            {
                get
                {
                    return this._targetPath.Value;
                }
            }

            private String GetValue(Object parent, String path)
            {
                Object element = parent;
                foreach (String property in path.Split('.'))
                {
                    PropertyInfo propertInfo;
                    if (element != null && (propertInfo = element.GetType().GetProperty(property)) != null)
                    {
                        element = propertInfo.GetValue(element, null);
                    }
                    else
                    {
                        return null;
                    }
                }
                return element.ToString();
            }

            private String GenerateUrl(String url, Object item)
            {
                MatchCollection matches = new Regex(@"\{([^\}]+)\}*").Matches(url);
                foreach (Match match in matches)
                {
                    String path = match.Groups[1].Value;
                    String value = this.GetValue(item, path);
                    url = url.Replace(match.Groups[0].Value, value);
                }
                return url;
            }

            public CatalogHierarchyTools(CatalogHierarchyPart catalogHierarchyPart, ICatalogHierarchyServices catalogHierarchyServices, IOrchardServices orchardServices)
            {
                this._orchardServices = orchardServices;
                this._catalogHieratchyPart = catalogHierarchyPart;
                this._catalogHierarchyServices = catalogHierarchyServices;
                this._targetPath = new Lazy<Uri>(
                    () =>
                    {
                        Uri targetPath;
                        if (String.IsNullOrEmpty(this._catalogHieratchyPart.TargetPath))
                        {
                            targetPath = this._orchardServices.WorkContext.HttpContext.Request.Url;
                        }
                        else
                        {
                            String path;
                            String leftPart = this._orchardServices.WorkContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                            if (this._catalogHieratchyPart.TargetPath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
                            {
                                path = String.Concat(VirtualPathUtility.ToAbsolute("~/"), this._catalogHieratchyPart.TargetPath.Substring(2));
                            }
                            else if (this._catalogHieratchyPart.TargetPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                            {
                                path = this._catalogHieratchyPart.TargetPath;
                            }
                            else
                            {
                                path = String.Concat(this._orchardServices.WorkContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Path), this._catalogHieratchyPart.TargetPath);
                            }
                            targetPath = new Uri(String.Concat(leftPart, path));
                        }
                        return targetPath;
                    }
                );
            }

            public String GenerateUrl(Object item)
            {
                String url;

                if (item is CatalogItemViewModel)
                {
                    url = this.GenerateUrl(this._catalogHieratchyPart.CatalogUrlPattern, ((CatalogItemViewModel)item).Catalog);
                }
                else if (item is CategoryItemViewModel)
                {
                    url = this.GenerateUrl(this._catalogHieratchyPart.CategoryUrlPattern, ((CategoryItemViewModel)item).Category);
                }
                else
                {
                    throw new NotSupportedException();
                }

                return url;
            }

            public String GetExpandUrl(Boolean expanded, String path)
            {
                return this._orchardServices.WorkContext.HttpContext.Request.Url.AddParameters(
                    new NameValueCollection 
                    { 
                        { this._catalogHierarchyServices.TargetParameterKey, this._catalogHieratchyPart.Id.ToString() }, 
                        { this._catalogHierarchyServices.ActionParameterKey, expanded ? this._catalogHierarchyServices.CollapseParameterValue : this._catalogHierarchyServices.ExpandParameterValue }, 
                        { this._catalogHierarchyServices.PathParameterKey, path } 
                    }
                ).ToString();
            }

            public String GetSelectUrl(String path)
            {
                return this._targetPath.Value.AddParameters(
                    new NameValueCollection 
                    { 
                        { this._catalogHierarchyServices.TargetParameterKey, this._catalogHieratchyPart.Id.ToString() }, 
                        { this._catalogHierarchyServices.ActionParameterKey, this._catalogHierarchyServices.SelectParameterValue }, 
                        { this._catalogHierarchyServices.PathParameterKey, path } 
                    }
                ).ToString();
            }
        }

        private Localizer _localizer;
        private IOrchardServices _orchardServices;
        private ICatalogHierarchyServices _catalogHierarchyServices;

        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_CatalogHierarchy";
            }
        }

        protected override DriverResult Display(CatalogHierarchyPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_CatalogHierarchy",
                () => shapeHelper.Parts_CatalogHierarchy(
                    CatalogHierarchyPartId: part.Id,
                    GenerateUrls: part.GenerateUrls,
                    Hierarchy: this._catalogHierarchyServices.GetModel(part),
                    PathSeparator: this._catalogHierarchyServices.PathSeparator,
                    Tools: new CatalogHierarchyTools(part, this._catalogHierarchyServices, this._orchardServices)
                )
            );
        }

        protected override DriverResult Editor(CatalogHierarchyPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_CatalogHierarchy_Edit",
                () => shapeHelper.EditorTemplate(
                    Model: part,
                    Prefix: this.Prefix,
                    TemplateName: "Parts/CatalogHierarchy"
                )
            );
        }

        protected override DriverResult Editor(CatalogHierarchyPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);

            if (part.GenerateUrls && String.IsNullOrEmpty(part.CategoryUrlPattern))
            {
                updater.AddModelError("CategoryUrlPatternRequired", this._localizer("Category url pattern is required"));
            }

            return this.Editor(part, shapeHelper);
        }

        public CatalogHierarchyPartDriver(ICatalogHierarchyServices catalogHierarchyServices, IOrchardServices orchardServices)
        {
            this._orchardServices = orchardServices;
            this._localizer = NullLocalizer.Instance;
            this._catalogHierarchyServices = catalogHierarchyServices;
        }
    }
}