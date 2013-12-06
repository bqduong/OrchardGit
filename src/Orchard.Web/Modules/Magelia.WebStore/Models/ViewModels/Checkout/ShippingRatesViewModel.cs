using Magelia.WebStore.Services.Contract.Data.Store;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Magelia.WebStore.Models.ViewModels.Checkout
{
    public class ShippingRatesViewModel
    {
        public class ShippingRateValueSelection
        {
            [Required]
            public String ShippingRateCode { get; set; }
            public Guid OrderSubsetId { get; set; }
        }

        private List<ShippingRateValueSelection> _shippingRateValueSelections;
        private Dictionary<Guid, IEnumerable<ShippingRateValue>> _shippingRatesByOrderSubset;

        public NumberFormatInfo NumberFormat { get; set; }

        public Dictionary<Guid, IEnumerable<ShippingRateValue>> ShippingRatesByOrderSubset
        {
            get
            {
                if (this._shippingRatesByOrderSubset == null)
                {
                    this.ShippingRatesByOrderSubset = new Dictionary<Guid, IEnumerable<ShippingRateValue>>();
                }
                return this._shippingRatesByOrderSubset;
            }
            set
            {
                this._shippingRatesByOrderSubset = value;
            }
        }

        public List<ShippingRateValueSelection> ShippingRateValueSelections
        {
            get
            {
                if (this._shippingRateValueSelections == null)
                {
                    this.ShippingRateValueSelections = new List<ShippingRateValueSelection>();
                }
                return this._shippingRateValueSelections;
            }
            set
            {
                this._shippingRateValueSelections = value;
            }
        }
    }
}