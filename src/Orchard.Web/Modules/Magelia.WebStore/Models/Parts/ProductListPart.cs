using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement;
using System;

namespace Magelia.WebStore.Models.Parts
{
    public class ProductListPart : ContentPart<ProductListPartRecord>
    {
        public Boolean EnablePaging
        {
            get
            {
                return this.Record.EnablePaging;
            }
            set
            {
                this.Record.EnablePaging = value;
            }
        }

        public Nullable<Int32> PageSize
        {
            get
            {
                return this.Record.PageSize;
            }
            set
            {
                this.Record.PageSize = value;
            }
        }

        public Boolean EnableSorting
        {
            get
            {
                return this.Record.EnableSorting;
            }
            set
            {
                this.Record.EnableSorting = value;
            }
        }

        public Boolean FromCatalogHierarchySelection
        {
            get
            {
                return this.Record.FromCatalogHierarchySelection;
            }
            set
            {
                this.Record.FromCatalogHierarchySelection = value;
            }
        }

        public Nullable<Int32> CatalogHierarchyId
        {
            get
            {
                return this.Record.CatalogHierarchyId;
            }
            set
            {
                this.Record.CatalogHierarchyId = value;
            }
        }

        public String CatalogCodeFilter
        {
            get
            {
                return this.Record.CatalogCodeFilter;
            }
            set
            {
                this.Record.CatalogCodeFilter = value;
            }
        }

        public String CategoryCodeFilter
        {
            get
            {
                return this.Record.CategoryCodeFilter;
            }
            set
            {
                this.Record.CategoryCodeFilter = value;
            }
        }

        public Boolean DisplayViewDetail
        {
            get
            {
                return this.Record.DisplayViewDetail;
            }
            set
            {
                this.Record.DisplayViewDetail = value;
            }
        }

        public String ViewDetailUrlPattern
        {
            get
            {
                return this.Record.ViewDetailUrlPattern;
            }
            set
            {
                this.Record.ViewDetailUrlPattern = value;
            }
        }

        public Boolean AllowAddToBasket
        {
            get
            {
                return this.Record.AllowAddToBasket;
            }
            set
            {
                this.Record.AllowAddToBasket = value;
            }
        }

        public Boolean DisplayProductsAvailability
        {
            get
            {
                return this.Record.DisplayProductsAvailability;
            }
            set
            {
                this.Record.DisplayProductsAvailability = value;
            }
        }
    }
}