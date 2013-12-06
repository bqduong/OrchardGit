using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement;
using System;

namespace Magelia.WebStore.Models.Parts
{
    public class BasketCountPart : ContentPart<BasketCountPartRecord>
    {
        public String BasketUrl
        {
            get
            {
                return this.Record.BasketUrl;
            }
            set
            {
                this.Record.BasketUrl = value;
            }
        }

        public Byte Count
        {
            get
            {
                return this.Record.Count;
            }
            set
            {
                this.Record.Count = value;
            }
        }
    }
}