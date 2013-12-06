using Magelia.WebStore.Services.Contract.Data.Customer;
using Orchard.Events;

namespace Magelia.WebStore.Events
{
    public interface IAddressEventHandler : IEventHandler
    {
        void Created(Address address);
        void Updated(Address address);
        void Removed(Address address);
    }
}