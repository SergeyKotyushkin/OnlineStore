using System.Web;
using OnlineStore.BuisnessLogic.Currency;
using OnlineStore.BuisnessLogic.Currency.Contracts;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.Database.Realizations;
using OnlineStore.BuisnessLogic.Lang;
using OnlineStore.BuisnessLogic.Lang.Contracts;
using OnlineStore.BuisnessLogic.Mail;
using OnlineStore.BuisnessLogic.Mail.Contracts;
using OnlineStore.BuisnessLogic.Models.Dto;
using OnlineStore.BuisnessLogic.OrderRepository;
using OnlineStore.BuisnessLogic.OrderRepository.Contracts;
using OnlineStore.BuisnessLogic.StorageRepository;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
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
            For<ICurrencyConverter>().Use<CurrencyConverter>().Singleton();
            For<ICurrencyService>().Use<CurrencyService>().Singleton();
            For<IRateService>().Use<YahooRateService>().Singleton();

            //// Mail
            For<IMailSender>().Use<MailSender>().AlwaysUnique();
            For<IMailService>().Use<MailService>().Singleton();

            //// Repository
            //For<IEfPersonRepository>().Use<EfPersonRepository>().Singleton();
            For<IDbProductRepository>().Use<EfProductRepository>().Singleton();
            For<IDbOrderHistoryRepository>().Use<EfOrderHistoryRepository>().Singleton();

            //// GridViews
            For<ITableManager<ProductDto>>().Use<TableAgent<ProductDto>>();
            For<ITableManager<OrderItemDto>>().Use<TableAgent<OrderItemDto>>();

            //// Other
            For<ILangSetter>().Use<LangSetter>().Singleton();
            //For<IImageService>().Use<ImageServiceAgent>();
            For<IUserGroup>().Use<UserGroup>();
            For<IStorageRepository<HttpSessionStateBase>>().Use<StorageSessionRepository>();
            For<IStorageRepository<HttpCookieCollection>>().Use<StorageCoockieRepository>();
            For<IOrderRepository<HttpSessionStateBase>>().Use<OrderSessionRepository>();
        }
    }
}