using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using OnlineStore.BuisnessLogic.Currency.Contracts;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.ElasticRepository.Contracts;
using OnlineStore.BuisnessLogic.MappingDtoExtensions;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.OrderRepository.Contracts;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;
using OnlineStore.MvcWebProject.Models.Catalog;
using OnlineStore.MvcWebProject.Utils.Attributes;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    [MyHandleError]
    [OnlyForRole("User")]
    public class CatalogController : Controller
    {
        private const int PageSize = 8;
        private const int VisiblePagesCount = 5;

        private readonly Color _successColor = Color.DarkGreen;
        private readonly Color _failColor = Color.Firebrick;

        private readonly CultureInfo _threadCulture = Thread.CurrentThread.CurrentCulture;

        private readonly PagerSettings _pagerSettings = new PagerSettings
        {
            PageChangeRoute = new Route {Action = "PageChange", Controller = "Catalog"},
            UpdateTargetId = "catalogTablePartial"
        };
        
        private readonly ITableManager<ProductDto, HttpSessionStateBase> _tableManager;
        private readonly IStorageRepository<HttpSessionStateBase> _storageSessionRepository;
        private readonly IStorageRepository<HttpCookieCollection> _storageCookieRepository;
        private readonly IOrderRepository<HttpSessionStateBase> _orderRepository;
        private readonly IDbProductRepository _dbProductRepository;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly IUserGroup _userGroup;
        private readonly IElasticRepository _elasticRepository;

        public CatalogController(ITableManager<ProductDto, HttpSessionStateBase> tableManager,
            IStorageRepository<HttpSessionStateBase> storageSessionRepository, IDbProductRepository dbProductRepository,
            IOrderRepository<HttpSessionStateBase> orderRepository,
            IStorageRepository<HttpCookieCollection> storageCookieRepository, ICurrencyConverter currencyConverter,
            IUserGroup userGroup, IElasticRepository elasticRepository)
        {
            _tableManager = tableManager;
            _storageSessionRepository = storageSessionRepository;
            _dbProductRepository = dbProductRepository;
            _orderRepository = orderRepository;
            _storageCookieRepository = storageCookieRepository;
            _currencyConverter = currencyConverter;
            _userGroup = userGroup;
            _elasticRepository = elasticRepository;
        }

        [HttpGet]
        public ActionResult Products()
        {
            ModelState.Clear();

            var name = Request.QueryString["name"];
            var category = Request.QueryString["category"];
            if (category == Lang.ProductCatalog_AllCategory) category = null;
            
            var model = new CatalogModel
            {
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

            try
            {
                _elasticRepository.CheckConnection();
            }
            catch (Exception ex)
            {
                model.Message = new Message { Text = ex.Message, Color = _failColor };
                return View(model);
            }

            RefreshSearchParameters(ref name, ref category);

            var pageIndex = _tableManager.GetOldPageIndexFromRepository(Session, Settings.Catalog_OldPageIndexName);
            var categories = GetCategories();

            model.TableData = GetTableData(pageIndex, name, category);
            model.Search = new Search
            {
                SearchName = name ?? string.Empty,
                SearchCategory = category,
                Categories =
                    categories.Select(c => new SelectListItem {Text = c, Value = c, Selected = c == category})
            };
            model.Message = MessageIfBought();

            return View(model);
        }
        
        public ActionResult PageChange(int pageindex)
        {
            _elasticRepository.CheckConnection();

            var name = Request.QueryString["name"];
            var category = Request.QueryString["category"];
            if (category == Lang.ProductCatalog_AllCategory) category = null;

            RefreshSearchParameters(ref name, ref category);
            
            var table = GetTableData(pageindex, name, category);

            return PartialView("_catalogTable", table);
        }
        
        public string AddToOrder(int id)
        {
            _elasticRepository.CheckConnection();

            var count = _orderRepository.Add(Session, Settings.OrderInStorage, id);
            return JsonConvert.SerializeObject(new {id, count});
        }

        public string RemoveFromOrder(int id)
        {
            _elasticRepository.CheckConnection();

            var count = _orderRepository.Remove(Session, Settings.OrderInStorage, id);
            return JsonConvert.SerializeObject(new { id, count });
        }


        private Table<ProductDto> GetTableData(int pageIndex, string name = null, string category = null)
        {
            var productsCount = _elasticRepository.GetCount(name, category);

            var pagesCount = _tableManager.GetPagesCount(productsCount, PageSize);

            var oldPageIndex = _tableManager.GetOldPageIndexFromRepository(Session, Settings.Catalog_OldPageIndexName);
            var newPageIndex = _tableManager.GetNewPageIndex(pageIndex, oldPageIndex, pagesCount);
            _tableManager.SetNewPageIndexToRepository(Session, Settings.Catalog_OldPageIndexName, newPageIndex);

            var productsDto = GetProductDtos(newPageIndex - 1, name, category);

            var pager = new Pager
            {
                PageIndex = newPageIndex,
                PagesCount = pagesCount,
                PageSize = PageSize,
                Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount),
                PagerSettings = _pagerSettings
            };

            return new Table<ProductDto> {Data = productsDto, Pager = pager};
        }

        private ProductDto[] GetProductDtos(int pageIndex, string name = null, string category = null)
        {
            var searchResults = _elasticRepository.SearchByNameAndCategory(name, category, pageIndex, PageSize);

            var products = searchResults == null
                ? _dbProductRepository.GetRange(pageIndex, PageSize)
                : _dbProductRepository.GetByIds(searchResults);
            
            var orders = _orderRepository.GetAll(Session, Settings.OrderInStorage);
            var culture = GetCurrencyCultureInfo();
            var rate = _currencyConverter.GetRate(CultureInfo.GetCultureInfo("ru-RU"), culture, DateTime.Now);
            
            return products.Select(p =>
            {
                var order = orders.FirstOrDefault(o => o.Id == p.Id);
                return p.ToProductDto(order == null ? 0 : order.Count, _currencyConverter, culture, rate);
            }).ToArray();
        }

        private IEnumerable<string> GetCategories()
        {
            var categories = _dbProductRepository.GetAll().Select(d => d.Category).Distinct().OrderBy(s => s).ToList();
            categories.Insert(0, Lang.ProductCatalog_AllCategory);
            return categories;
        }

        private void RefreshSearchParameters(ref string name, ref string category)
        {
            if (name == null)
            {
                name = (string)_storageSessionRepository.Get(Session, Settings.Catalog_SearchNameInStorage);
                category = (string)_storageSessionRepository.Get(Session, Settings.Catalog_SearchCategoryInStorage);
            }
            else if (!CheckSearchParametersIsFreshInStorage(name, category))
            {
                _storageSessionRepository.Set(Session, Settings.Catalog_SearchNameInStorage, name);
                _storageSessionRepository.Set(Session, Settings.Catalog_SearchCategoryInStorage, category);
            }
        }

        private bool CheckSearchParametersIsFreshInStorage(string name, string category)
        {
            var sName = (string)_storageSessionRepository.Get(Session, Settings.Catalog_SearchNameInStorage);
            var sCategory = (string)_storageSessionRepository.Get(Session, Settings.Catalog_SearchCategoryInStorage);

            return name == sName && category == sCategory;
        }

        private Message MessageIfBought()
        {
            if (Session[Settings.BoughtInStorage] == null) 
                return null;

            Session[Settings.BoughtInStorage] = null;
            return new Message {Text = Lang.ProductCatalog_ProductsBought, Color = _successColor};
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