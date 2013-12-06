using System;
using System.Globalization;
using Contract = Magelia.WebStore.Services.Contract.Data.Store;

namespace Magelia.WebStore.Models.ViewModels.Basket
{
    public class BasketViewModel
    {
        public String Message { get; set; }
        public Boolean ReadOnly { get; set; }
        public String CheckoutUrl { get; set; }
        public Contract.Basket Basket { get; set; }
        public String CurrentPromoCode { get; set; }
        public NumberFormatInfo NumberFormat { get; set; }
    }
}