using Orchard.ContentManagement.Records;
using System;

namespace Magelia.WebStore.Models.Records
{
    public class BasketCountPartRecord : ContentPartRecord
    {
        public virtual Byte Count { get; set; }
        public virtual String BasketUrl { get; set; }
    }
}