using Magelia.WebStore.Models.Parts;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Magelia.WebStore.Models.ViewModels.ProductList
{
    public class ProductListEditViewModel
    {
        private IEnumerable<SelectListItem> _catalogHierarchies;

        public ProductListPart Part { get; set; }

        public IEnumerable<SelectListItem> CatalogHierarchies
        {
            get
            {
                if (this._catalogHierarchies == null)
                {
                    this.CatalogHierarchies = Enumerable.Empty<SelectListItem>();
                }
                return this._catalogHierarchies;
            }
            set
            {
                this._catalogHierarchies = value;
            }
        }
    }
}