using Microsoft.AspNet.SignalR.Client.Hubs;
using System;

namespace Magelia.WebStore.Client
{
    public abstract partial class WebStoreContextBase : WebStoreContext<ICatalogServiceClient>
    {
        #region Nested classes

        public class ExtendedWebStoreHubs : WebStoreHubs
        {
            public ExtendedWebStoreHubs(ILogger logger)
                : base(logger)
            { }

            #region Hubs

            #endregion

            protected override void CreateHubs(HubConnection connection)
            {
                base.CreateHubs(connection);
            }
        }

        #endregion

        #region Static members

        private static Lazy<ExtendedWebStoreHubs> _hubs;

        public static ExtendedWebStoreHubs Hubs
        {
            get
            {
                return WebStoreContext._hubs.Value;
            }
        }

        static WebStoreContextBase()
        {
            DependencyResolver.Instance.RegisterFactory<ExtendedWebStoreHubs>(() => new ExtendedWebStoreHubs(DependencyResolver.Instance.Resolve<ILogger>()));

            WebStoreContext._hubs = new Lazy<ExtendedWebStoreHubs>(
                () =>
                {
                    ExtendedWebStoreHubs hubs = DependencyResolver.Instance.Resolve<ExtendedWebStoreHubs>();
                    hubs.Reset(DependencyResolver.Instance.Resolve<WebStoreContextSettings>());
                    return hubs;
                }
            );
        }

        #endregion

        #region Constructors

        public WebStoreContextBase(WebStoreContextSettings settings)
            : base(settings)
        { 
        }

        #endregion

        #region ExtensionService

        #endregion
    }
}
