using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement;
using System;
using System.ComponentModel.DataAnnotations;

namespace Magelia.WebStore.Models.Parts
{
    public class SettingsPart : ContentPart<SettingsPartRecord>
    {
        [Required]
        public Guid StoreId
        {
            get
            {
                return this.Record.StoreId;
            }
            set
            {
                this.Record.StoreId = value;
            }
        }

        [Required]
        [StringLength(256)]
        [RegularExpression(@"^http(s)?\:\/\/.+$")]
        public String ServicesPath
        {
            get
            {
                return this.Record.ServicesPath;
            }
            set
            {
                this.Record.ServicesPath = value;
            }
        }

        public Boolean AllowRegionNavigation
        {
            get
            {
                return this.Record.AllowRegionNavigation;
            }
            set
            {
                this.Record.AllowRegionNavigation = value;
            }
        }
    }
}