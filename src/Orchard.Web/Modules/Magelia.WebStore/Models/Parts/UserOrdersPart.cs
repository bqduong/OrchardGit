using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement;
using System;

namespace Magelia.WebStore.Models.Parts
{
    public class UserOrdersPart : ContentPart<UserOrdersPartRecord>
    {
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
    }
}