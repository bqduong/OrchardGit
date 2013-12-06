using System;

namespace Magelia.WebStore.Client
{
    public class WebStoreContext : WebStoreContextBase
    {
        private Nullable<Guid> _regionId;
        private Nullable<Int32> _cultureId;
        private Nullable<Int32> _countryId;
        private Nullable<Int32> _currencyId;
        private ICatalogServiceClient _catalogClient;

        public ICatalogServiceClient CatalogClient
        {
            get
            {
                if (this._catalogClient == null)
                {
                    this._catalogClient = this.GetCatalogClient(this._cultureId, this._currencyId, this._countryId, this._regionId, null);
                }
                return this._catalogClient;
            }
        }

        protected override ICatalogServiceClient CreateCatalogServiceClient(ICatalogContext catalogContext)
        {
            return new EnhancedCatalogServiceClient(new Lazy<IStoreServiceClient>(() => this.StoreClient), catalogContext);
        }

        public WebStoreContext(WebStoreContextSettings settings)
            : base(settings)
        {
        }

        public WebStoreContext(WebStoreContextSettings settings, Int32 cultureId, Int32 currencyId, Int32 countryId, Nullable<Guid> regionId)
            : base(settings)
        {
            this._regionId = regionId;
            this._cultureId = cultureId;
            this._countryId = countryId;
            this._currencyId = currencyId;
        }
    }
}