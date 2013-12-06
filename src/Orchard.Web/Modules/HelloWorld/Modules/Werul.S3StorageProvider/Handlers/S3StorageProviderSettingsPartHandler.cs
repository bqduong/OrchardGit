using JetBrains.Annotations;
using Werul.S3StorageProvider.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Werul.S3StorageProvider.Handlers {
    [UsedImplicitly]
    public class S3StorageProviderSettingsPartHandler : ContentHandler {
        public S3StorageProviderSettingsPartHandler(IRepository<S3StorageProviderSettingsRecord> repository)
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<S3StorageProviderSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Amazon S3")));
        }
    }
}