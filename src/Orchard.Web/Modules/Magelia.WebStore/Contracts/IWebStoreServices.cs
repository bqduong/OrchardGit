using Magelia.WebStore.Client;
using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard;
using Orchard.Security;
using System;
using System.Globalization;

namespace Magelia.WebStore.Contracts
{
    public interface IWebStoreServices : IDependency
    {
        String BasketName { get; }
        Boolean IsAnonymous { get; }
        String CurrentUserName { get; }
        Int32 CurrentCultureId { get; }
        String AnonymousUserName { get; }
        StoreContext StoreContext { get; }
        Int32 CurrentCountryId { get; set; }
        Int32 CurrentCurrencyId { get; set; }
        NumberFormatInfo NumberFormat { get; }
        Nullable<Guid> CurrentRegionId { get; set; }

        void EnsureUser(IUser user);
        Exception UsingClient(Action<WebStoreContext> action);
        Exception UsingClient(Action<WebStoreContext> action, Boolean contextualize);
        Exception UsingClient(Guid storeId, String servicesPath, Action<WebStoreContext> action);
    }
}
