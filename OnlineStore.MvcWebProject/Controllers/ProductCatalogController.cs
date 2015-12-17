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
using OnlineStore.BuisnessLogic.MappingDto;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.OrderRepository.Contracts;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.Models.ProductCatalog;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    [Authorize(Roles = "User")]
    public class ProductCatalogController : Controller
    {
        private const int PageSize = 8;
        private const int VisiblePagesCount = 5;
        private const string OrderInStorage = "CurrentOrder";
        private const string SearchNameInStorage = "SearchNamePC";
        private const string SearchCategoryInStorage = "SearchCategoryPC";
        private const string OldPageIndexName = "CurrentPageIndexPC";
        private const string BoughtInStorage = "Bought";

        private readonly Color _successColor = Color.DarkGreen;

        private readonly ITableManager<ProductDto> _tableManager;
        private readonly IStorageRepository<HttpSessionStateBase> _storageSessionRepository;
        private readonly IStorageRepository<HttpCookieCollection> _storageCookieRepository;
        private readonly IOrderRepository<HttpSessionStateBase> _orderRepository;
        private readonly IDbProductRepository _dbProductRepository;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly IUserGroup _userGroup;

        public ProductCatalogController(ITableManager<ProductDto> tableManager,
            IStorageRepository<HttpSessionStateBase> storageSessionRepository, IDbProductRepository dbProductRepository,
            IOrderRepository<HttpSessionStateBase> orderRepository,
            IStorageRepository<HttpCookieCollection> storageCookieRepository, ICurrencyConverter currencyConverter,
            IUserGroup userGroup)
        {
            _tableManager = tableManager;
            _storageSessionRepository = storageSessionRepository;
            _dbProductRepository = dbProductRepository;
            _orderRepository = orderRepository;
            _storageCookieRepository = storageCookieRepository;
            _currencyConverter = currencyConverter;
            _userGroup = userGroup;
        }

        [HttpGet]
        public ActionResult ProductList()
        {
            ModelState.Clear();

            var name = Request.QueryString["name"];
            var category = Request.QueryString["category"];
            if (category == Lang.ProductCatalog_AllCategory) category = null;

            RefreshSearchParameters(ref name, ref category);

            var id = (_storageSessionRepository.Get(Session, OldPageIndexName) ?? 1).ToString();
            var categoryList = CreateCategoryList().ToList();

            var model = new ProductCatalogModel
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
                    SelectedLanguage = Thread.CurrentThread.CurrentUICulture.Name,
                    SelectedCurrency = (_storageCookieRepository.Get(Request.Cookies, Lang.CurrencyInStorage) ??
                                        Thread.CurrentThread.CurrentUICulture.Name).ToString()
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

            return PartialView("_productCatalogList", table);
        }
        
        public string AddToOrder(int id)
        {
            var count = _orderRepository.Add(Session, OrderInStorage, id);
            return JsonConvert.SerializeObject(new {id, count});
        }

        public string RemoveFromOrder(int id)
        {
            var count = _orderRepository.Remove(Session, OrderInStorage, id);
            return JsonConvert.SerializeObject(new { id, count });
        }


        private Table<ProductDto> GetTableData(string id, string name = null, string category = null)
        {
            var productsDto = GetProductDtoList(name, category);

            var pagesCount = GetPagesCount(productsDto);

            var newPageIndex = GetNewPageIndex(id, pagesCount);

            var data = _tableManager.GetPageData(productsDto, newPageIndex, PageSize).ToArray();
            var pager = new Pager
            {
                PageIndex = newPageIndex,
                PagesCount = pagesCount,
                PageSize = PageSize,
                PagerVisible = true,
                Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount),
                PagerSettings =
                    new PagerSettings
                    {
                        PageChangeRoute = new Route { Action = "PageChange", Controller = "ProductCatalog" },
                        UpdateTargetId = "productCatalogTablePartial"
                    }
            };

            return new Table<ProductDto> {Data = data, Pager = pager};
        }

        private List<ProductDto> GetProductDtoList(string name = null, string category = null)
        {
            var products = _dbProductRepository.GetAll();
            if (!string.IsNullOrEmpty(name)) products = _dbProductRepository.SearchByName(products, name);
            if (!string.IsNullOrEmpty(category)) products = _dbProductRepository.SearchByCategory(products, category);

            var orders = _orderRepository.GetAll(Session, OrderInStorage);
            var currencyCultureName =
                (_storageCookieRepository.Get(Request.Cookies, Lang.CurrencyInStorage) ??
                 Thread.CurrentThread.CurrentUICulture.Name).ToString();
            var culture = CultureInfo.GetCultureInfo(currencyCultureName);

            var rate = _currencyConverter.GetRate(CultureInfo.GetCultureInfo("ru-RU"), culture, DateTime.Now);
            foreach (var p in products)
                p.Price = _currencyConverter.ConvertByRate(p.Price, rate);
            
            return products.Select(p =>
            {
                var order = orders.FirstOrDefault(o => o.Id == p.Id);
                return ProductDtoMapping.ToDto(p, order == null ? 0 : order.Count, culture);
            }).ToList();
        }

        private static int GetPagesCount(IReadOnlyCollection<ProductDto> products)
        {
            return (int)Math.Ceiling((double)products.Count / PageSize);
        }

        private int GetNewPageIndex(string id, int pagesCount)
        {
            var oldPageIndex = (int)(_storageSessionRepository.Get(Session, OldPageIndexName) ?? 1);
            var newPageIndex = _tableManager.GetPageIndex(id, oldPageIndex, pagesCount);
            _storageSessionRepository.Set(Session, OldPageIndexName, newPageIndex);

            return newPageIndex;
        }

        private IEnumerable<string> CreateCategoryList()
        {
            var data = _dbProductRepository.GetAll();
            var categories = data.Select(d => d.Category).Distinct().OrderBy(s => s).ToList();
            categories.Insert(0, Lang.ProductCatalog_AllCategory);
            return categories;
        }

        private bool CheckSearchParametersIsFreshInStorage(string name, string category)
        {
            var sName = (string) _storageSessionRepository.Get(Session, SearchNameInStorage);
            var sCategory = (string) _storageSessionRepository.Get(Session, SearchCategoryInStorage);

            return name == sName && category == sCategory;
        }

        private void SaveSearchParametersInStorage(string name, string category)
        {
            _storageSessionRepository.Set(Session, SearchNameInStorage, name);
            _storageSessionRepository.Set(Session, SearchCategoryInStorage, category);
        }
        
        private void RefreshSearchParameters(ref string name, ref string category)
        {
            if (name == null)
            {
                name = (string)_storageSessionRepository.Get(Session, SearchNameInStorage);
                category = (string)_storageSessionRepository.Get(Session, SearchCategoryInStorage);
            }
            else if (!CheckSearchParametersIsFreshInStorage(name, category))
                SaveSearchParametersInStorage(name, category);
        }

        private Message MessageIfBought()
        {
            if (Session[BoughtInStorage] == null) 
                return null;

            Session[BoughtInStorage] = null;
            return new Message { Text = Lang.ProductCatalog_ProductsBought, Color = _successColor };
        }
    }
}
