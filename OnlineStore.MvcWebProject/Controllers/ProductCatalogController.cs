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

namespace OnlineStore.MvcWebProject.Controllers
{
    [Authorize(Roles = "User")]
    public class ProductCatalogController : Controller
    {
        private const int PageSize = 8;
        private const int VisiblePagesCount = 5;
        private const string OrderStorageName = "CurrentOrder";
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
        public ActionResult ProductList(ProductCatalogModel model, string id)
        {
            ModelState.Clear();

            var table = GetTableData(id);
            var search = new Search
            {
                CategoryList = new SelectList(new[] { "a", "b", "c" })
            };

            return View(new ProductCatalogModel { TableData = table, Search = search });
        }

        private Table<ProductDto> GetTableData(string id)
        {
            var productsDto = GetProductDtoList();

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
        
        public PartialViewResult PageChange(int pageindex)
        {
            var table = GetTableData(pageindex.ToString());

            return PartialView("_productCatalogList", table);
        }

        public string AddToOrder(int id)
        {
            var count = _orderRepository.Add(Session, OrderStorageName, id);
            return JsonConvert.SerializeObject(new {id, count});
        }

        public string RemoveFromOrder(int id)
        {
            var count = _orderRepository.Remove(Session, OrderStorageName, id);
            return JsonConvert.SerializeObject(new { id, count });
        }
        
        private List<ProductDto> GetProductDtoList()
        {
            var products = _dbProductRepository.GetAll().ToList();
            var orders = _orderRepository.GetAll(Session, OrderStorageName);
            var culture = CultureInfo.CurrentCulture;
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
            var oldPageIndex = (int)(_storageRepository.Get(Session, OldPageIndexName) ?? 1);
            var newPageIndex = _tableManager.GetPageIndex(id, oldPageIndex, pagesCount);
            _storageRepository.Set(Session, OldPageIndexName, newPageIndex);

            return newPageIndex;
        }
    }
}
