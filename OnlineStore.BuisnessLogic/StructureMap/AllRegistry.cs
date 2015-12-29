using System.Web;
using OnlineStore.BuisnessLogic.Currency;
using OnlineStore.BuisnessLogic.Currency.Contracts;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.Database.Realizations;
using OnlineStore.BuisnessLogic.ElasticRepository.Contracts;
using OnlineStore.BuisnessLogic.ImageService;
using OnlineStore.BuisnessLogic.ImageService.Contracts;
using OnlineStore.BuisnessLogic.JsonSerialize;
using OnlineStore.BuisnessLogic.JsonSerialize.Contracts;
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
            For<IRateService>().Use<YahooRateService>().Singleton();
            For<ICurrencyService>().Use<CurrencyService>().Singleton();
            For<ICurrencyConverter>().Use<CurrencyConverter>().Singleton();

            //// Mail
            For<IMailService>().Use<MailService>().Singleton();
            For<IMailSender>().Use<MailSender>().AlwaysUnique();

            //// Repository
            For<IDbPersonRepository>().Use<EfPersonRepository>().Singleton();
            For<IDbProductRepository>().Use<EfProductRepository>().Singleton();
            For<IDbOrderHistoryRepository>().Use<EfOrderHistoryRepository>().Singleton();
            For<IElasticRepository>().Use<ElasticRepository.ElasticRepository>();

            //// GridViews
            For<ITableManagement>().Use<TableManagement>();
            For<ITableManager<ProductDto, HttpSessionStateBase>>().Use<TableAgent<ProductDto, HttpSessionStateBase>>();
            For<ITableManager<OrderItemDto, HttpSessionStateBase>>().Use<TableAgent<OrderItemDto, HttpSessionStateBase>>();
            For<ITableManager<OrderHistoryItemDto, HttpSessionStateBase>>().Use<TableAgent<OrderHistoryItemDto, HttpSessionStateBase>>();
            For<ITableManager<ProductManagementDto, HttpSessionStateBase>>().Use<TableAgent<ProductManagementDto, HttpSessionStateBase>>();

            //// Other
            For<IUserGroup>().Use<UserGroup>();
            For<IJsonSerializer>().Use<JsonSerializer>();
            For<IImageService>().Use<ImageServiceAgent>();
            For<IOrderRepository<HttpSessionStateBase>>().Use<OrderSessionRepository>();
            For<IStorageRepository<HttpSessionStateBase>>().Use<StorageSessionRepository>();
            For<IStorageRepository<HttpCookieCollection>>().Use<StorageCoockieRepository>();
        }
    }
}