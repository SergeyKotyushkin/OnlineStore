using System;
using System.Collections.Generic;
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
using OnlineStore.BuisnessLogic.MappingDtoExtensions;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;
using OnlineStore.MvcWebProject.Models;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
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

        private static readonly ILog Log = LogManager.GetLogger(typeof(HomeController));

        private readonly IStorageRepository<HttpSessionStateBase> _storageSessionRepository;
        private readonly IStorageRepository<HttpCookieCollection> _storageCookieRepository;
        private readonly IUserGroup _userGroup;
        private readonly IDbProductRepository _dbProductRepository;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly ITableManager<ProductManagementDto, HttpSessionStateBase> _tableManager;
        private readonly ITableManagement _tableManagement;

        public ManagementController(IStorageRepository<HttpSessionStateBase> storageSessionRepository,
            IStorageRepository<HttpCookieCollection> storageCookieRepository, IUserGroup userGroup,
            IDbProductRepository dbProductRepository, ICurrencyConverter currencyConverter,
            ITableManager<ProductManagementDto, HttpSessionStateBase> tableManager, ITableManagement tableManagement)
        {
            _storageSessionRepository = storageSessionRepository;
            _storageCookieRepository = storageCookieRepository;
            _userGroup = userGroup;
            _dbProductRepository = dbProductRepository;
            _currencyConverter = currencyConverter;
            _tableManager = tableManager;
            _tableManagement = tableManagement;
        }

        public ActionResult Index()
        {
            var id = (_storageSessionRepository.Get(Session, Settings.Management_OldPageIndexName) ?? 1).ToString();

            var model = new ManagementModel
            {
                TableData = GetTableData(id),
                Settings = new MainLayoutSettings
                {
                    Title = Lang.ProductCatalog_Title,
                    MoneyVisible = true,
                    LinkProfileText = string.Format(Lang.MainLayout_LinkProfileText, _userGroup.GetUser().UserName),
                    LogoutVisible = true,
                    SelectedLanguage = _threadCulture.Name,
                    SelectedCurrency = GetCurrencyCultureInfo().Name
                }
            };

            return View(model);
        }

        public PartialViewResult PageChange(int pageindex)
        {
            var table = GetTableData(pageindex.ToString());

            return PartialView("_managementTable", table);
        }

        public string Add(string jsonProduct)
        {
            var productAnonymous =
                new { Id = string.Empty, Name = string.Empty, Category = string.Empty, Price = string.Empty };
            var product = JsonConvert.DeserializeAnonymousType(jsonProduct, productAnonymous);

            var culture = GetCurrencyCultureInfo();

            var result = _tableManagement.AddOrUpdateProduct(null, product.Name, product.Category, product.Price, culture);

            Message message;
            switch (result)
            {
                case EditingResults.Success:
                    message = new Message {Text = Lang.Management_AddSuccess, Color = _successColor};
                    Log.Info(string.Format("Product {0} successfully added.", product.Name));
                    break;
                case EditingResults.FailValidProduct:
                    message = new Message {Text = Lang.Management_AddValidFail, Color = _failColor};
                    break;
                case EditingResults.FailAddOrUpdate:
                    message = new Message {Text = Lang.Management_AddFail, Color = _failColor};
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var products = GetProducManagementDtoList();

            _storageSessionRepository.Set(Session, Settings.Management_OldPageIndexName, _tableManager.GetPagesCount(products.Count, PageSize));

            return JsonConvert.SerializeObject(new {result = result == EditingResults.Success, message});
        }

        public string Delete(int id)
        {
            var product = _dbProductRepository.GetById(id);

            Message message;
            if (_dbProductRepository.RemoveById(id))
            {
                message = new Message {Text = Lang.Management_DeleteSuccess, Color = _successColor};
                Log.Info(string.Format("Product {0} successfully removed.", product.Name));
            }
            else
                message = new Message {Text = Lang.Management_DeleteFail, Color = _failColor};

            return JsonConvert.SerializeObject(new {result = true, message});
        }
        
        public string Edit(string jsonProduct)
        {
            var productAnonymous =
                new {Id = string.Empty, Name = string.Empty, Category = string.Empty, Price = string.Empty};
            var product = JsonConvert.DeserializeAnonymousType(jsonProduct, productAnonymous);

            var culture = GetCurrencyCultureInfo();

            var result = _tableManagement.AddOrUpdateProduct(product.Id, product.Name, product.Category, product.Price, culture);

            Message message;
            switch (result)
            {
                case EditingResults.Success:
                    message = new Message {Text = Lang.Management_EditSuccess, Color = _successColor};
                    Log.Info(string.Format("Product {0} successfully edited.", product.Name));
                    break;
                case EditingResults.FailValidProduct:
                    message = new Message {Text = Lang.Management_EditValidFail, Color = _failColor};
                    break;
                case EditingResults.FailAddOrUpdate:
                    message = new Message { Text = Lang.Management_EditFail, Color = _failColor };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return JsonConvert.SerializeObject(new {message});
        }

        public PartialViewResult RefresshTable()
        {
            var pageIndex = (_storageSessionRepository.Get(Session, Settings.Management_OldPageIndexName) ?? 1).ToString();

            var table = GetTableData(pageIndex);

            return PartialView("_managementTable", table);
        }


        private Table<ProductManagementDto> GetTableData(string id)
        {
            var producManagementDtoList = GetProducManagementDtoList();

            var pagesCount = _tableManager.GetPagesCount(producManagementDtoList.Count, PageSize);

            var newPageIndex = _tableManager.GetNewPageIndex(Session, Settings.Management_OldPageIndexName, id, pagesCount);

            var data = _tableManager.GetPageData(producManagementDtoList, newPageIndex, PageSize);

            var pager = new Pager
            {
                PageIndex = newPageIndex,
                PagesCount = pagesCount,
                PageSize = PageSize,
                Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount),
                PagerSettings = _pagerSettings
            };

            return new Table<ProductManagementDto> { Data = data, Pager = pager };
        }

        private List<ProductManagementDto> GetProducManagementDtoList()
        {
            var products = _dbProductRepository.GetAll();

            var culture = GetCurrencyCultureInfo();

            var rate = _currencyConverter.GetRate(CultureInfo.GetCultureInfo("ru-RU"), culture, DateTime.Now);
            foreach (var p in products)
                p.Price = _currencyConverter.ConvertByRate(p.Price, rate);

            return products.Select(p => p.ToProductManagementDto(culture)).ToList();
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
