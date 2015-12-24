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
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.MappingDtoExtensions;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.OrderRepository.Contracts;
using OnlineStore.BuisnessLogic.Searching.Contracts;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;
using OnlineStore.MvcWebProject.Models.Catalog;
using OnlineStore.MvcWebProject.Utils.Attributes;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    [OnlyForRole("User")]
    public class CatalogController : Controller
    {
        private const int PageSize = 8;
        private const int VisiblePagesCount = 5;

        private readonly Color _successColor = Color.DarkGreen;

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
        private readonly ISearching<Product> _searching;

        public CatalogController(ITableManager<ProductDto, HttpSessionStateBase> tableManager,
            IStorageRepository<HttpSessionStateBase> storageSessionRepository, IDbProductRepository dbProductRepository,
            IOrderRepository<HttpSessionStateBase> orderRepository,
            IStorageRepository<HttpCookieCollection> storageCookieRepository, ICurrencyConverter currencyConverter,
            IUserGroup userGroup, ISearching<Product> searching)
        {
            _tableManager = tableManager;
            _storageSessionRepository = storageSessionRepository;
            _dbProductRepository = dbProductRepository;
            _orderRepository = orderRepository;
            _storageCookieRepository = storageCookieRepository;
            _currencyConverter = currencyConverter;
            _userGroup = userGroup;
            _searching = searching;
        }

        [HttpGet]
        public ActionResult Products()
        {
            ModelState.Clear();

            var name = Request.QueryString["name"];
            var category = Request.QueryString["category"];
            if (category == Lang.ProductCatalog_AllCategory) category = null;

            RefreshSearchParameters(ref name, ref category);

            var id = (_storageSessionRepository.Get(Session, Settings.Catalog_OldPageIndexName) ?? 1).ToString();
            var categoryList = CreateCategoryList();

            var model = new CatalogModel
            {
                TableData = GetTableData(id, name, category),
                Search = new Search
                {
                    SearchName = name ?? string.Empty,
                    SearchCategory = category,
                    CategoryList =
                        categoryList.Select(c => new SelectListItem {Text = c, Value = c, Selected = c == category})
                },
                Settings = new MainLayoutSettings
                {
                    Title = Lang.ProductCatalog_Title,
                    MoneyVisible = true,
                    LinkProfileText = string.Format(Lang.MainLayout_LinkProfileText, _userGroup.GetUser().UserName),
                    LogoutVisible = true,
                    SelectedLanguage = _threadCulture.Name,
                    SelectedCurrency = (_storageCookieRepository.Get(Request.Cookies, Settings.CurrencyInStorage) ??
                                        _threadCulture.Name).ToString()
                },
                Message = MessageIfBought()
            };

            return View(model);
        }

        public PartialViewResult PageChange(int pageindex)
        {
            var name = Request.QueryString["name"];
            var category = Request.QueryString["category"];
            if (category == Lang.ProductCatalog_AllCategory) category = null;

            RefreshSearchParameters(ref name, ref category);

            var table = GetTableData(pageindex.ToString(), name, category);

            return PartialView("_catalogTable", table);
        }
        
        public string AddToOrder(int id)
        {
            var count = _orderRepository.Add(Session, Settings.OrderInStorage, id);
            return JsonConvert.SerializeObject(new {id, count});
        }

        public string RemoveFromOrder(int id)
        {
            var count = _orderRepository.Remove(Session, Settings.OrderInStorage, id);
            return JsonConvert.SerializeObject(new { id, count });
        }


        private Table<ProductDto> GetTableData(string id, string name = null, string category = null)
        {
            var productsDto = GetProductDtoList(name, category);

            var pagesCount = _tableManager.GetPagesCount(productsDto.Count, PageSize);

            var newPageIndex = _tableManager.GetNewPageIndex(Session, Settings.Catalog_OldPageIndexName, id, pagesCount);

            var data = _tableManager.GetPageData(productsDto, newPageIndex, PageSize);

            var pager = new Pager
            {
                PageIndex = newPageIndex,
                PagesCount = pagesCount,
                PageSize = PageSize,
                Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount),
                PagerSettings = _pagerSettings
            };

            return new Table<ProductDto> {Data = data, Pager = pager};
        }

        private List<ProductDto> GetProductDtoList(string name = null, string category = null)
        {
            var products = _dbProductRepository.GetAll();
            products = _searching.Search(products, name, category);

            var orders = _orderRepository.GetAll(Session, Settings.OrderInStorage);
            var currencyCultureName =
                (_storageCookieRepository.Get(Request.Cookies, Settings.CurrencyInStorage) ?? _threadCulture.Name)
                    .ToString();
            var culture = CultureInfo.GetCultureInfo(currencyCultureName);

            var rate = _currencyConverter.GetRate(CultureInfo.GetCultureInfo("ru-RU"), culture, DateTime.Now);
            
            return products.Select(p =>
            {
                var order = orders.FirstOrDefault(o => o.Id == p.Id);
                return p.ToProductDto(order == null ? 0 : order.Count, _currencyConverter, culture, rate);
            }).ToList();
        }

        private IEnumerable<string> CreateCategoryList()
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
    }
}