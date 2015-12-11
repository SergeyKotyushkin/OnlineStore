using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.MappingDto;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.OrderRepository.Contracts;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.MvcWebProject.Models.ProductCatalog;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    [Authorize(Roles = "User")]
    public class ProductCatalogController : Controller
    {
        private const int PageSize = 8;
        private const int VisiblePagesCount = 5;
        private const string OrderInStorage = "CurrentOrderPC";
        private const string SearchNameInStorage = "SearchNamePC";
        private const string SearchCategoryInStorage = "SearchCategoryPC";
        private const string OldPageIndexName = "CurrentPageIndexPC";

        private readonly ITableManager<ProductDto> _tableManager;
        private readonly IStorageRepository<HttpSessionStateBase> _storageRepository;
        private readonly IOrderRepository<HttpSessionStateBase> _orderRepository;
        private readonly IDbProductRepository _dbProductRepository;

        public ProductCatalogController(ITableManager<ProductDto> tableManager,
            IStorageRepository<HttpSessionStateBase> storageRepository, IDbProductRepository dbProductRepository,
            IOrderRepository<HttpSessionStateBase> orderRepository)
        {
            _tableManager = tableManager;
            _storageRepository = storageRepository;
            _dbProductRepository = dbProductRepository;
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public ActionResult ProductList()
        {
            ModelState.Clear();

            var name = Request.QueryString["name"];
            var category = Request.QueryString["category"];
            if (category == Lang.ProductCatalog_AllCategory) category = null;

            RefreshSearchParameters(ref name, ref category);

            var id = (_storageRepository.Get(Session, OldPageIndexName) ?? 1).ToString();
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
                    ProfileVisible = true,
                    LogoutVisible = true
                }
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
                Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount)
            };

            return new Table<ProductDto> {Data = data, Pager = pager};
        }


        private List<ProductDto> GetProductDtoList(string name = null, string category = null)
        {
            var products = _dbProductRepository.GetAll();
            if (!string.IsNullOrEmpty(name)) products = _dbProductRepository.SearchByName(products, name);
            if (!string.IsNullOrEmpty(category)) products = _dbProductRepository.SearchByCategory(products, category);
            var productList = products.ToList();

            var orders = _orderRepository.GetAll(Session, OrderInStorage);
            var culture = CultureInfo.CurrentCulture;
            return productList.Select(p =>
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
            var oldPageIndex = (int)(_storageRepository.Get(Session, OldPageIndexName) ?? 1);
            var newPageIndex = _tableManager.GetPageIndex(id, oldPageIndex, pagesCount);
            _storageRepository.Set(Session, OldPageIndexName, newPageIndex);

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
            var sName = (string) _storageRepository.Get(Session, SearchNameInStorage);
            var sCategory = (string) _storageRepository.Get(Session, SearchCategoryInStorage);

            return name == sName && category == sCategory;
        }

        private void SaveSearchParametersInStorage(string name, string category)
        {
            _storageRepository.Set(Session, SearchNameInStorage, name);
            _storageRepository.Set(Session, SearchCategoryInStorage, category);
        }
        
        private void RefreshSearchParameters(ref string name, ref string category)
        {
            if (name == null)
            {
                name = (string)_storageRepository.Get(Session, SearchNameInStorage);
                category = (string)_storageRepository.Get(Session, SearchCategoryInStorage);
            }
            else if (!CheckSearchParametersIsFreshInStorage(name, category))
                SaveSearchParametersInStorage(name, category);
        }
    }
}
