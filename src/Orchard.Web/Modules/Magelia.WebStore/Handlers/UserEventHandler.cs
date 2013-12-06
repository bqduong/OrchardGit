using Magelia.WebStore.Contracts;
using Magelia.WebStore.Events;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Services.Contract.Parameters.Store;
using Orchard;
using Orchard.Security;
using Orchard.Users.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Magelia.WebStore.Handlers
{
    public class UserEventHandler : IUserEventHandler
    {
        private IOrchardServices _orchardServices;
        private IWebStoreServices _webStoreServices;
        private IUserModelsStateServices _userModelsStateServices;
        private IEnumerable<IBasketEventHandler> _basketEventHandlers;

        private void TransfertAndUpdateBasket(String fromUserName, String toUsername)
        {
            this._webStoreServices.UsingClient(
                wc =>
                {
                    String basketName = wc.BasketClient.TransferBaskets(fromUserName, toUsername, new[] { this._webStoreServices.BasketName }).FirstOrDefault();
                    wc.BasketClient.UpdateBasket(toUsername, this._webStoreServices.BasketName, this._webStoreServices.CurrentCurrencyId, CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).LCID, new Location { CountryId = this._webStoreServices.CurrentCountryId, RegionId = this._webStoreServices.CurrentRegionId });

                    if (String.Equals(this._webStoreServices.BasketName, basketName))
                    {
                        this._basketEventHandlers.Trigger(h => h.Transfered(fromUserName, toUsername));
                    }
                }
            );
        }

        public UserEventHandler(IWebStoreServices webStoreServices, IOrchardServices orchardServices, IUserModelsStateServices userModelsStateServices, IEnumerable<IBasketEventHandler> basketEventHandlers)
        {
            this._orchardServices = orchardServices;
            this._webStoreServices = webStoreServices;
            this._basketEventHandlers = basketEventHandlers;
            this._userModelsStateServices = userModelsStateServices;
        }

        public void Creating(UserContext context)
        {
        }

        public void Created(UserContext context)
        {
            this._webStoreServices.EnsureUser(context.User);
        }

        public void LoggedIn(IUser user)
        {
            this._userModelsStateServices.FlushUserContext();
            this._webStoreServices.EnsureUser(user);
            this.TransfertAndUpdateBasket(this._webStoreServices.AnonymousUserName, user.UserName);
        }

        public void LoggedOut(IUser user)
        {
            this._userModelsStateServices.FlushUserContext();
            this.TransfertAndUpdateBasket(user.UserName, this._webStoreServices.AnonymousUserName);
        }

        public void AccessDenied(IUser user)
        {
        }

        public void ChangedPassword(IUser user)
        {
        }

        public void SentChallengeEmail(IUser user)
        {
        }

        public void ConfirmedEmail(IUser user)
        {
        }

        public void Approved(IUser user)
        {
        }
    }
}