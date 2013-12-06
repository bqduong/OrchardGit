using Orchard.ContentManagement.Records;
using System;

namespace Magelia.WebStore.Models.Records
{
    public class BasketPartRecord : ContentPartRecord
    {
        public virtual Boolean ReadOnly { get; set; }
        public virtual String CheckoutUrl { get; set; }
    }
}