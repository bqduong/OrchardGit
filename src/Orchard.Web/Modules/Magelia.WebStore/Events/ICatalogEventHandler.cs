using Magelia.WebStore.Client;
using Orchard.Events;
using System.Collections.Generic;

namespace Magelia.WebStore.Events
{
    public interface ICatalogEventHandler : IEventHandler
    {
        void CatalogRetrieving(List<ExtendedCatalog> catalogs);
    }
}
