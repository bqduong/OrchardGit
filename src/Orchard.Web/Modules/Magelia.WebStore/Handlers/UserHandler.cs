using Magelia.WebStore.Contracts;
using Magelia.WebStore.Events;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Services.Contract.Data.Customer;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;
using Orchard.Services;
using Orchard.Tasks.Scheduling;
using System.Collections.Generic;

namespace Magelia.WebStore.Handlers
{
    public class UserHandler : IContentHandler
    {
        private IClock _clock;
        private IWebStoreServices _webStoreServices;
        private IScheduledTaskManager _scheduledTaskManager;
        private IEnumerable<ICustomerEventHandler> _customerEventHandlers;

        public UserHandler(IWebStoreServices webStoreServices, IScheduledTaskManager scheduledTaskManager, IClock clock, IEnumerable<ICustomerEventHandler> customerEventHandlers)
        {
            this._clock = clock;
            this._webStoreServices = webStoreServices;
            this._scheduledTaskManager = scheduledTaskManager;
            this._customerEventHandlers = customerEventHandlers;
        }

        public void Activating(ActivatingContentContext context)
        {
        }

        public void Activated(ActivatedContentContext context)
        {
        }

        public void Initializing(InitializingContentContext context)
        {
        }

        public void Initialized(InitializingContentContext context) 
        {
        }

        public void Creating(CreateContentContext context)
        {
        }

        public void Created(CreateContentContext context)
        {
        }

        public void Loading(LoadContentContext context)
        {
        }

        public void Loaded(LoadContentContext context)
        {
        }

        public void Updating(UpdateContentContext context)
        {
        }

        public void Updated(UpdateContentContext context)
        {
            IUser user = context.ContentManager.Get<IUser>(context.Id);
            if (user != null)
            {
                this._scheduledTaskManager.CreateTask("SychronizeUser", this._clock.UtcNow.AddMinutes(1), context.ContentItem);
            }
        }

        public void Versioning(VersionContentContext context)
        {
        }

        public void Versioned(VersionContentContext context)
        {
        }

        public void Publishing(PublishContentContext context)
        {
        }

        public void Published(PublishContentContext context)
        {
        }

        public void Unpublishing(PublishContentContext context)
        {
        }

        public void Unpublished(PublishContentContext context)
        {
        }

        public void Removing(RemoveContentContext context)
        {
        }

        public void Removed(RemoveContentContext context)
        {
            IUser user = context.ContentManager.Get<IUser>(context.Id);

            if (user != null)
            {
                this._webStoreServices.UsingClient(
                    c =>
                    {
                        Customer customer = c.CustomerClient.GetCustomer(user.UserName, false);

                        if (customer != null && c.CustomerClient.DeleteCustomer(user.UserName))
                        {
                            this._customerEventHandlers.Trigger(h => h.Removed(customer));
                        }
                    }
                );
            }
        }

        public void Indexing(IndexContentContext context)
        {
        }

        public void Indexed(IndexContentContext context)
        {
        }

        public void Importing(ImportContentContext context)
        {
        }

        public void Imported(ImportContentContext context)
        {
        }

        public void Exporting(ExportContentContext context)
        {
        }

        public void Exported(ExportContentContext context)
        {
        }

        public void GetContentItemMetadata(GetContentItemMetadataContext context)
        {
        }

        public void BuildDisplay(BuildDisplayContext context)
        {
        }

        public void BuildEditor(BuildEditorContext context)
        {
        }

        public void UpdateEditor(UpdateEditorContext context)
        {
        }
    }
}