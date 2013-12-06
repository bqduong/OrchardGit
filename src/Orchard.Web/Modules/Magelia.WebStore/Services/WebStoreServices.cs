using Magelia.WebStore.Client;
using Magelia.WebStore.Contracts;
using Magelia.WebStore.Events;
using Magelia.WebStore.Extensions;
using Magelia.WebStore.Models.Parts;
using Magelia.WebStore.Services.Contract.Data.Customer;
using Magelia.WebStore.Services.Contract.Data.Store;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Security;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Security;

namespace Magelia.WebStore.Services
{
    public class WebStoreServices : IWebStoreServices
    {
        private const String StoreContextHttpCacheKey = "storecontext";
        private const String CurrentRegionIdSessionKey = "currentregionid";
        private const String CurrentCountryIdSessionKey = "currentcountryid";
        private const String CurrentCurrencyIdSessionKey = "currentcurrencyid";
        private const String AnonymousUserNameCookieKey = "mageliaanonymoususername";

        private String _anonymousUserName;
        private NumberFormatInfo _numberFormat;
        private Orchard.Logging.ILogger _logger;
        private IOrchardServices _orchardServices;
        private IUserModelsStateServices _userModelsStateServices;

        private Nullable<Int32> _currentCountryId
        {
            get
            {
                return this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentCountryIdSessionKey] as Nullable<Int32>;
            }
            set
            {
                this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentCountryIdSessionKey] = value;
            }
        }

        private Nullable<Guid> _currentRegionId
        {
            get
            {
                return this._webStoreSettings.AllowRegionNavigation ? this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentRegionIdSessionKey] as Nullable<Guid> : null;
            }
            set
            {
                this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentRegionIdSessionKey] = value;
            }
        }

        private Nullable<Int32> _currentCurrencyId
        {
            get
            {
                return this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentCurrencyIdSessionKey] as Nullable<Int32>;
            }
            set
            {
                this._orchardServices.WorkContext.HttpContext.Session[WebStoreServices.CurrentCurrencyIdSessionKey] = value;
            }
        }

        private SettingsPart _webStoreSettings
        {
            get
            {
                return this._orchardServices.WorkContext.CurrentSite.As<SettingsPart>();
            }
        }

        public String BasketName
        {
            get
            {
                return "default";
            }
        }

        public NumberFormatInfo NumberFormat
        {
            get
            {
                if (this._numberFormat == null)
                {
                    this._numberFormat = this.GetNumberFormat();
                }
                return this._numberFormat;
            }
        }

        public StoreContext StoreContext
        {
            get
            {
                StoreContext defaultStoreContext = this.GetStoreContext(null);
                Culture culture = this.GetCorrespondingCulture(defaultStoreContext);
                return culture == null ? defaultStoreContext : this.GetStoreContext(new CultureInfo(this._orchardServices.WorkContext.CurrentCulture).LCID);
            }
        }

        public String AnonymousUserName
        {
            get
            {
                if (String.IsNullOrEmpty(this._anonymousUserName))
                {
                    this._anonymousUserName = this.EnsureAnonymousUserName();
                }
                return this._anonymousUserName;
            }
        }

        public Boolean IsAnonymous
        {
            get
            {
                return this._orchardServices.WorkContext.CurrentUser == null;
            }
        }

        public String CurrentUserName
        {
            get
            {
                return this.IsAnonymous ? this.AnonymousUserName : this._orchardServices.WorkContext.CurrentUser.UserName;
            }
        }

        public Int32 CurrentCountryId
        {
            get
            {
                if (!this.StoreContext.AvailableCountries.Any(ac => ac.CountryId == this._currentCountryId))
                {
                    this._currentCountryId = this.StoreContext.AvailableCountries.OrderByDescending(ac => ac.IsDefault).Select(ac => ac.CountryId).FirstOrDefault();
                }
                return this._currentCountryId.Value;
            }
            set
            {
                if (this._currentCountryId != value)
                {
                    this._currentRegionId = null;
                    this._currentCountryId = value;

                    this._userModelsStateServices.FlushCommerceContext();
                }
            }
        }

        public Nullable<Guid> CurrentRegionId
        {
            get
            {
                return this._currentRegionId;
            }
            set
            {
                this._currentRegionId = value;
                this._userModelsStateServices.FlushCommerceContext();
            }
        }

        public Int32 CurrentCurrencyId
        {
            get
            {
                if (!this.StoreContext.AvailableCurrencies.Any(ac => ac.CurrencyId == this._currentCurrencyId))
                {
                    this._currentCurrencyId = this.StoreContext.AvailableCurrencies.Where(ac => ac.IsDefault).Select(ac => ac.CurrencyId).FirstOrDefault();
                }
                return this._currentCurrencyId.Value;
            }
            set
            {
                if (this._currentCurrencyId != value)
                {
                    this._currentCurrencyId = value;
                    this._userModelsStateServices.FlushCommerceContext();
                }
            }
        }

        public Int32 CurrentCultureId
        {
            get
            {
                Culture culture = this.GetCorrespondingCulture(this.StoreContext);
                return culture == null ? this.StoreContext.AvailableCultures.OrderByDescending(ac => ac.IsDefault).Select(ac => ac.LCID).FirstOrDefault() : culture.LCID;
            }
        }

        private String EnsureAnonymousUserName()
        {
            HttpCookie cookie;

            if (HttpContext.Current.Request.Cookies.AllKeys.Contains(WebStoreServices.AnonymousUserNameCookieKey))
            {
                cookie = HttpContext.Current.Request.Cookies[WebStoreServices.AnonymousUserNameCookieKey];
            }
            else
            {
                cookie = new HttpCookie(WebStoreServices.AnonymousUserNameCookieKey, Guid.NewGuid().ToString());
                HttpContext.Current.Response.Cookies.Add(cookie);
                this.UsingClient(c => c.CustomerClient.CreateAnonymousCustomer(cookie.Value));
            }
            cookie.Expires = DateTime.Now.AddYears(1);

            return cookie.Value;
        }

        private NumberFormatInfo GetNumberFormat()
        {
            NumberFormatInfo numberFormat = CultureInfo.GetCultureInfo(this._orchardServices.WorkContext.CurrentCulture).NumberFormat.Clone() as NumberFormatInfo;
            numberFormat.CurrencySymbol = this.StoreContext.AvailableCurrencies.Where(ac => ac.CurrencyId == this.CurrentCurrencyId).Select(c => c.Symbol).FirstOrDefault();
            return numberFormat;
        }

        private Culture GetCorrespondingCulture(Magelia.WebStore.Services.Contract.Data.Store.StoreContext storeContext)
        {
            return storeContext.AvailableCultures.FirstOrDefault(ac => ac.NetName.EqualsOrdinalIgnoreCase(this._orchardServices.WorkContext.CurrentCulture));
        }

        private Magelia.WebStore.Services.Contract.Data.Store.StoreContext GetStoreContext(Nullable<Int32> cultureId)
        {
            String storeContextCacheKey = String.Format("{0}-{1}", WebStoreServices.StoreContextHttpCacheKey, cultureId);
            StoreContext storeContext = HttpContext.Current.Cache.Get(storeContextCacheKey) as StoreContext;

            if (storeContext == null)
            {
                this.Execute(
                    () => this.NewClient(false),
                    c =>
                    {
                        storeContext = c.StoreClient.GetContext(cultureId);
                        HttpContext.Current.Cache.Add(
                            storeContextCacheKey,
                            storeContext,
                            null,
                            DateTime.Now.AddHours(1),
                            Cache.NoSlidingExpiration,
                            CacheItemPriority.Normal,
                            null
                        );
                    }
                );
            }

            return storeContext;
        }

        private WebStoreContext NewClient(Boolean contextualize)
        {
            return this.NewClient(this._webStoreSettings.StoreId, this._webStoreSettings.ServicesPath, contextualize);
        }

        private WebStoreContext NewClient(Guid storeId, String servicesPath, Boolean contextualize)
        {
            WebStoreContextSettings settings = new WebStoreContextSettings(storeId, new Uri(servicesPath));
            return contextualize ? new WebStoreContext(settings, this.CurrentCultureId, this.CurrentCurrencyId, this.CurrentCountryId, this.CurrentRegionId) : new WebStoreContext(settings);
        }

        private Exception Execute(Func<WebStoreContext> clientBuilder, Action<WebStoreContext> action)
        {
            try
            {
                Stopwatch watch = Stopwatch.StartNew();
                using (WebStoreContext client = clientBuilder())
                {
                    action(client);
                }
                watch.Stop();
                this._logger.Debug(String.Format("Magelia Client : {0} ms", watch.ElapsedMilliseconds));
            }
            catch (Exception exception)
            {
                this._logger.Error(exception, null);
                return exception;
            }
            return null;
        }

        public WebStoreServices(IOrchardServices orchardServices, IUserModelsStateServices userModelsStateServices)
        {
            this._logger = NullLogger.Instance;
            this._orchardServices = orchardServices;
            this._userModelsStateServices = userModelsStateServices;
        }

        public Exception UsingClient(Action<WebStoreContext> action)
        {
            return this.UsingClient(action, true);
        }

        public Exception UsingClient(Action<WebStoreContext> action, Boolean contextualize)
        {
            return this.Execute(() => this.NewClient(contextualize), action);
        }

        public Exception UsingClient(Guid storeId, String servicesPath, Action<WebStoreContext> action)
        {
            return this.Execute(() => this.NewClient(storeId, servicesPath, false), action);
        }

        public void EnsureUser(IUser user)
        {
            this.UsingClient(
                c =>
                {
                    if (c.CustomerClient.GetCustomer(user.UserName, true) == null)
                    {
                        MembershipCreateStatus status;
                        Customer customer = c.CustomerClient.CreateCustomer(user.UserName, Guid.NewGuid().ToString(), String.IsNullOrEmpty(user.Email) ? null : user.Email, null, null, true, out status);
                        if (status == MembershipCreateStatus.Success && customer != null)
                        {
                            this._orchardServices.WorkContext.Resolve<System.Collections.Generic.IEnumerable<ICustomerEventHandler>>().Trigger(h => h.Created(customer));
                        }
                    }
                }
            );
        }
    }
}