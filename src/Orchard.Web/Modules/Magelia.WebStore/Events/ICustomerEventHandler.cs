using Magelia.WebStore.Services.Contract.Data.Customer;
using Orchard.Events;

namespace Magelia.WebStore.Events
{
    public interface ICustomerEventHandler : IEventHandler
    {
        void Created(Customer customer);
        void Updated(Customer customer);
        void Removed(Customer customer);
    }
}