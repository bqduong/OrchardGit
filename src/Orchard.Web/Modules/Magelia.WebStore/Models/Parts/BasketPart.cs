using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement;
using System;

namespace Magelia.WebStore.Models.Parts
{
    public class BasketPart : ContentPart<BasketPartRecord>
    {
        public Boolean ReadOnly
        {
            get
            {
                return this.Record.ReadOnly;
            }
            set
            {
                this.Record.ReadOnly = value;
            }
        }

        public String CheckoutUrl
        {
            get
            {
                return this.Record.CheckoutUrl;
            }
            set
            {
                this.Record.CheckoutUrl = value;
            }
        }
    }
}