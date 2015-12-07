using OnlineStore.BuisnessLogic.UserGruop;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using StructureMap;
using StructureMap.Graph;

namespace OnlineStore.BuisnessLogic.StructureMap
{
    public class AllRegistry : Registry
    {
        public AllRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            // Currency
            //For<ICurrencyConverter>().Use<CurrencyConverter>().Singleton();
            //For<ICurrencyService>().Use<CurrencyService>().Singleton();
            //For<IRateService>().Use<YahooRateService>().Singleton();

            //// Mail
            //For<IMailSender>().Use<MailSender>().AlwaysUnique();
            //For<IMailService>().Use<MailService>().Singleton();

            //// Repository
            //For<IEfPersonRepository>().Use<EfPersonRepository>().Singleton();
            //For<IEfProductRepository>().Use<EfProductRepository>().Singleton();
            //For<IEfOrderHistoryRepository>().Use<EfOrderHistoryRepository>().Singleton();

            //// GridViews
            //For<IGridViewProductCatalogManager<HttpSessionState>>().Use<GridViewProductCatalogAgent<HttpSessionState>>();
            //For<IGridViewBasketManager<HttpSessionState>>().Use<GridViewBasketAgent<HttpSessionState>>();
            //For<IGridViewProductManagementManager<HttpSessionState>>().Use<GridViewProductManagementAgent<HttpSessionState>>();
            //For<IGridViewProfileManager<HttpSessionState>>().Use<GridViewProfileAgent<HttpSessionState>>();

            //// Other
            //For<ILangSetter>().Use<LangSetter>().Singleton();
            //For<ICurrencyCultureService<HttpCookieCollection>>().Use<CurrencyCultureCookieService>();
            //For<IStorageService<HttpSessionState>>().Use<StorageSessionService>();
            //For<IOrderRepository<HttpSessionState>>().Use<OrderSessionRepository>();
            //For<IImageService>().Use<ImageServiceAgent>();
            For<IUserGroup>().Use<UserGroup>();
        }
    }
}