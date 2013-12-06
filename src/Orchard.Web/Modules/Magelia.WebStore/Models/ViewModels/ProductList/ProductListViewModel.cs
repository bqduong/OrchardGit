using Magelia.WebStore.Client;
using Magelia.WebStore.Services.Contract.Data.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Magelia.WebStore.Models.ViewModels.ProductList
{
    public class ProductListViewModel : List<BaseProduct>
    {
        public class ProductListState
        {
            public Int32 PageCount { get; set; }
            public String FromPath { get; set; }
            public Nullable<Int32> Page { get; set; }
            public String SortExpression { get; set; }
            public String CatalogCodeFilter { get; set; }
            public String CategoryCodeFilter { get; set; } 
            public Nullable<SortDirection> SortDirection { get; set; }
        }

        private IEnumerable<Stock> _inventories;
        private Dictionary<String, IEnumerable<Client.Attribute>> _derivedAttributes;

        public ProductListState State { get; set; }

        public IEnumerable<Stock> Inventories
        {
            get
            {
                if (this._inventories == null)
                {
                    this.Inventories = Enumerable.Empty<Stock>();
                }
                return this._inventories;
            }
            set
            {
                this._inventories = value;
            }
        }

        public Dictionary<String, IEnumerable<Client.Attribute>> DerivedAttributes
        {
            get
            {
                if (this._derivedAttributes == null)
                {
                    this.DerivedAttributes = new Dictionary<String, IEnumerable<Client.Attribute>>();
                }
                return this._derivedAttributes;
            }
            set
            {
                this._derivedAttributes = value;
            }
        }
    }
}