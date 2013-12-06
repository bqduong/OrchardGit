using Magelia.WebStore.Services.Contract.Data.Store;
using System.Collections.Generic;
using System.Globalization;

namespace Magelia.WebStore.Models.ViewModels.Checkout
{
    public class ShippingRatesRecapViewModel : List<ShippingRateValue>
    {
        public NumberFormatInfo NumberFormat { get; set; }
    }
}