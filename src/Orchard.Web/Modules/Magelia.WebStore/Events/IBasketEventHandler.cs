using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard.Events;
using System;

namespace Magelia.WebStore.Events
{
    public interface IBasketEventHandler : IEventHandler
    {
        void Cleared();
        void SavedAsOrder(Order order);
        void PromoCodeAdded(PromoCodeEntryResult result);
        void PromoCodeRemoved(PromoCodeEntryResult result);
        void ProductAddedToBasket(BasketEntryResult result);
        void ProductQuantityUpdated(BasketEntryResult result);
        void Transfered(String fromUsername, String toUsername);
    }
}