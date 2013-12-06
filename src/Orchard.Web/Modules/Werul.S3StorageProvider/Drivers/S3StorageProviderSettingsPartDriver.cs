using System;
using Werul.S3StorageProvider.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Werul.S3StorageProvider.Drivers
{
    public class S3StorageProviderSettingsPartDriver : ContentPartDriver<S3StorageProviderSettingsPart> {
        public S3StorageProviderSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "S3StorageProviderSettings"; } }

        protected override DriverResult Editor(S3StorageProviderSettingsPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(S3StorageProviderSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {

            return ContentShape("Parts_S3StorageProvider_SiteSettings", () =>
            {
                    if (updater != null) {
                        updater.TryUpdateModel(part.Record, Prefix, null, null);
                    }
                    return shapeHelper.EditorTemplate(TemplateName: "Parts.S3StorageProvider.SiteSettings", Model: part.Record, Prefix: Prefix); 
                })
                .OnGroup("Amazon S3");
        }
    }
}