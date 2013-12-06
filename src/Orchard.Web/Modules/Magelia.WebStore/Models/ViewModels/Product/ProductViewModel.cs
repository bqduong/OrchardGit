using Magelia.WebStore.Client;
using Magelia.WebStore.Services.Contract.Data.Store;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Magelia.WebStore.Models.ViewModels.Product
{
    public class ProductViewModel
    {
        private BaseProduct _baseProduct;
        private IEnumerable<Stock> _inventories;

        public String RequestedSKU { get; set; }
        public String RequestedCatalogCode { get; set; }
        public ReferenceProduct ReferenceProduct { get; set; }

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

        public BaseProduct BaseProduct
        {
            get
            {
                if (this._baseProduct == null && this.ReferenceProduct != null)
                {
                    this.BaseProduct = this.ReferenceProduct is VariantProduct ? (this.ReferenceProduct as VariantProduct).VariableProduct as BaseProduct : this.ReferenceProduct;
                }
                return this._baseProduct;
            }
            private set 
            {
                this._baseProduct = value;
            }
        }
    }
}