using Magelia.WebStore.Contracts;
using Magelia.WebStore.Events;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Services.Contract.Data.Customer;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Tasks.Scheduling;
using System.Collections.Generic;

namespace Magelia.WebStore.Handlers
{
    public class UserSynchronizationHandler : IScheduledTaskHandler
    {
        private IWebStoreServices _webStoreServices;
        private IEnumerable<ICustomerEventHandler> _customerEventHandlers;

        public UserSynchronizationHandler(IWebStoreServices webStoreClient, IEnumerable<ICustomerEventHandler> customerEventHandlers)
        {
            this._webStoreServices = webStoreClient;
            this._customerEventHandlers = customerEventHandlers;
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == "SychronizeUser")
            {
                IUser user = context.Task.ContentItem.As<IUser>();
                this._webStoreServices.UsingClient(
                    c =>
                    { 
                        Customer customer = c.CustomerClient.GetCustomer(user.UserName, false);
                        if(customer != null)
                        {
                            c.CustomerClient.UpdateCustomer(customer.CustomerId, user.UserName, user.Email, true);
                            this._customerEventHandlers.Trigger(h => h.Updated(customer));
                        }
                    },
                    false
                );
            }
        }
    }
}