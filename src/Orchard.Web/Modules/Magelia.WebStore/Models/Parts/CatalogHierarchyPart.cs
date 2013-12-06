using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement;
using System;

namespace Magelia.WebStore.Models.Parts
{
    public class CatalogHierarchyPart : ContentPart<CatalogHierarchyPartRecord>
    {
        public Boolean GenerateUrls
        {
            get
            {
                return this.Record.GenerateUrls;
            }
            set
            {
                this.Record.GenerateUrls = value;
            }
        }

        public String CatalogUrlPattern
        {
            get
            {
                return this.Record.CatalogUrlPattern;
            }
            set
            {
                this.Record.CatalogUrlPattern = value;
            }
        }

        public String CategoryUrlPattern
        {
            get
            {
                return this.Record.CategoryUrlPattern;
            }
            set
            {
                this.Record.CategoryUrlPattern = value;
            }
        }

        public String TargetPath
        {
            get
            {
                return this.Record.TargetPath;
            }
            set
            {
                this.Record.TargetPath = value;
            }
        }
    }
}