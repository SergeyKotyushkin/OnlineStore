using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using log4net;
using Newtonsoft.Json;
using OnlineStore.BuisnessLogic.Currency.Contracts;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.ElasticRepository.Contracts;
using OnlineStore.BuisnessLogic.MappingDtoExtensions;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;
using OnlineStore.MvcWebProject.Models;
using OnlineStore.MvcWebProject.Utils.Attributes;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    [MyHandleError]
    [OnlyForRole("Admin")]
    public class ManagementController : Controller
    {
        private const int PageSize = 10;
        private const int VisiblePagesCount = 10;

        private readonly Color _successColor = Color.DarkGreen;
        private readonly Color _failColor = Color.Firebrick;

        private readonly CultureInfo _threadCulture = Thread.CurrentThread.CurrentCulture;

        private readonly PagerSettings _pagerSettings = new PagerSettings
        {
            PageChangeRoute = new Route {Action = "PageChange", Controller = "Management"},
            UpdateTargetId = "managementTablePartial"
        };

        private static readonly ILog Log = LogManager.GetLogger(typeof(ManagementController));

        private readonly IStorageRepository<HttpSessionStateBase> _storageSessionRepository;
        private readonly IStorageRepository<HttpCookieCollection> _storageCookieRepository;
        private readonly IUserGroup _userGroup;
        private readonly IDbProductRepository _dbProductRepository;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly ITableManager<ProductManagementDto, HttpSessionStateBase> _tableManager;
        private readonly ITableManagement _tableManagement;
        private readonly IElasticRepository _elasticRepository;

        public ManagementController(IStorageRepository<HttpSessionStateBase> storageSessionRepository,
            IStorageRepository<HttpCookieCollection> storageCookieRepository, IUserGroup userGroup,
            IDbProductRepository dbProductRepository, ICurrencyConverter currencyConverter,
            ITableManager<ProductManagementDto, HttpSessionStateBase> tableManager, ITableManagement tableManagement,
            IElasticRepository elasticRepository)
        {
            _storageSessionRepository = storageSessionRepository;
            _storageCookieRepository = storageCookieRepository;
            _userGroup = userGroup;
            _dbProductRepository = dbProductRepository;
            _currencyConverter = currencyConverter;
            _tableManager = tableManager;
            _tableManagement = tableManagement;
            _elasticRepository = elasticRepository;
        }

        public ActionResult Index()
        {
            var model = new ManagementModel
            {
                Settings = new MainLayoutSettings
                {
                    Title = Lang.Management_Title,
                    MoneyVisible = true,
                    LinkProfileText = string.Format(Lang.MainLayout_LinkProfileText, _userGroup.GetUser().UserName),
                    LogoutVisible = true,
                    SelectedLanguage = _threadCulture.Name,
                    SelectedCurrency = GetCurrencyCultureInfo().Name
                }
            };

            try
            {
                _elasticRepository.CheckConnection();
            }
            catch(Exception ex)
            {
                model.Message = new Message {Text = ex.Message, Color = _failColor};
                return View(model);
            }
            
            var pageIndex = _tableManager.GetOldPageIndexFromRepository(Session, Settings.Management_OldPageIndexName);
            model.TableData = GetTableData(pageIndex);

            return View(model);
        }
        
        public ActionResult PageChange(int pageindex)
        {
            _elasticRepository.CheckConnection();

            var table = GetTableData(pageindex);

            return PartialView("_managementTable", table);
        }
        
        public string Add(string jsonProduct)
        {
            _elasticRepository.CheckConnection();

            var productAnonymous =
                new { Id = string.Empty, Name = string.Empty, Category = string.Empty, Price = string.Empty };
            var product = JsonConvert.DeserializeAnonymousType(jsonProduct, productAnonymous);

            var culture = GetCurrencyCultureInfo();

            var result = _tableManagement.AddOrUpdateProduct(null, product.Name, product.Category, product.Price, culture);

            Message message;
            if (result.EditingResult == EditingResults.Success)
            {
                _elasticRepository.AddOrUpdate(new ProductElasticDto(result.Product.Id, product.Name, product.Category));
                message = new Message {Text = Lang.Management_AddSuccess, Color = _successColor};
                Log.Info(string.Format("Product {0} successfully added.", product.Name));
            }
            else if (result.EditingResult == EditingResults.FailValidProduct)
                message = new Message {Text = Lang.Management_AddValidFail, Color = _failColor};
            else
                message = new Message {Text = Lang.Management_AddFail, Color = _failColor};

            _storageSessionRepository.Set(Session, Settings.Management_OldPageIndexName,
                _tableManager.GetPagesCount(_elasticRepository.GetCount(), PageSize));

            return JsonConvert.SerializeObject(new {result = result.EditingResult == EditingResults.Success, message});
        }
        
        public string Delete(int id)
        {
            _elasticRepository.CheckConnection();

            var product = _dbProductRepository.GetById(id);

            Message message;
            if (_dbProductRepository.RemoveById(id) != null)
            {
                _elasticRepository.RemoveById(id);
                message = new Message {Text = Lang.Management_DeleteSuccess, Color = _successColor};
                Log.Info(string.Format("Product {0} successfully removed.", product.Name));
            }
            else
                message = new Message {Text = Lang.Management_DeleteFail, Color = _failColor};

            return JsonConvert.SerializeObject(new {result = true, message});
        }
        
        public string Edit(string jsonProduct)
        {
            _elasticRepository.CheckConnection();

            var productAnonymous =
                new {Id = string.Empty, Name = string.Empty, Category = string.Empty, Price = string.Empty};
            var product = JsonConvert.DeserializeAnonymousType(jsonProduct, productAnonymous);

            var culture = GetCurrencyCultureInfo();

            var result = _tableManagement.AddOrUpdateProduct(product.Id, product.Name, product.Category, product.Price, culture);

            Message message;
            if (result.EditingResult == EditingResults.Success)
            {
                _elasticRepository.AddOrUpdate(new ProductElasticDto(result.Product.Id, product.Name, product.Category));
                message = new Message {Text = Lang.Management_EditSuccess, Color = _successColor};
                Log.Info(string.Format("Product {0} successfully edited.", product.Name));
            }
            else if (result.EditingResult == EditingResults.FailValidProduct)
                message = new Message {Text = Lang.Management_EditValidFail, Color = _failColor};
            else
                message = new Message {Text = Lang.Management_EditFail, Color = _failColor};

            return JsonConvert.SerializeObject(new {message});
        }

        public ActionResult RefresshTable()
        {
            _elasticRepository.CheckConnection();

            var pageIndex = _tableManager.GetOldPageIndexFromRepository(Session, Settings.Management_OldPageIndexName);

            var table = GetTableData(pageIndex);

            return PartialView("_managementTable", table);
        }


        private Table<ProductManagementDto> GetTableData(int pageIndex)
        {
            var productsCount = _elasticRepository.GetCount();

            var pagesCount = _tableManager.GetPagesCount(productsCount, PageSize);

            var oldPageIndex = _tableManager.GetOldPageIndexFromRepository(Session, Settings.Management_OldPageIndexName);
            var newPageIndex = _tableManager.GetNewPageIndex(pageIndex, oldPageIndex, pagesCount);
            _tableManager.SetNewPageIndexToRepository(Session, Settings.Management_OldPageIndexName, newPageIndex);

            var producManagementDtoList = GetProducManagementDtos(newPageIndex-1);
            
            var pager = new Pager
            {
                PageIndex = newPageIndex,
                PagesCount = pagesCount,
                PageSize = PageSize,
                Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount),
                PagerSettings = _pagerSettings
            };

            return new Table<ProductManagementDto> { Data = producManagementDtoList, Pager = pager };
        }

        private ProductManagementDto[] GetProducManagementDtos(int pageIndex)
        {
            var products = _dbProductRepository.GetRange(pageIndex, PageSize);

            var culture = GetCurrencyCultureInfo();

            var rate = _currencyConverter.GetRate(CultureInfo.GetCultureInfo("ru-RU"), culture, DateTime.Now);
            foreach (var p in products)
                p.Price = _currencyConverter.ConvertByRate(p.Price, rate);

            return products.Select(p => p.ToProductManagementDto(culture)).ToArray();
        }

        private CultureInfo GetCurrencyCultureInfo()
        {
            var currencyCultureName =
                (_storageCookieRepository.Get(Request.Cookies, Settings.CurrencyInStorage) ??
                 _threadCulture.Name).ToString();
            return CultureInfo.GetCultureInfo(currencyCultureName);
        }
    }
}
