using Orchard.ContentManagement.Records;
using System;

namespace Magelia.WebStore.Models.Records
{
    public class CatalogHierarchyPartRecord : ContentPartRecord
    {
        public virtual String TargetPath { get; set; }
        public virtual Boolean GenerateUrls { get; set; }
        public virtual String CatalogUrlPattern { get; set; }
        public virtual String CategoryUrlPattern { get; set; }
    }
}