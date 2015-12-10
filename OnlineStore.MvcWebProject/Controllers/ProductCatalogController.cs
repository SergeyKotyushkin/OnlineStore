using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Models;
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
        private const string OldPageIndexName = "CurrentPageIndexPC";

        private readonly ITableManager<Product> _tableManager;
        private readonly IStorageRepository<HttpSessionStateBase> _storageRepository;
        private readonly IDbProductRepository _dbProductRepository;

        public ProductCatalogController(ITableManager<Product> tableManager,
            IStorageRepository<HttpSessionStateBase> storageRepository, IDbProductRepository dbProductRepository)
        {
            _tableManager = tableManager;
            _storageRepository = storageRepository;
            _dbProductRepository = dbProductRepository;
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

        private Table<Product> GetTableData(string id)
        {
            var products = _dbProductRepository.GetAll().ToList();
            //new List<Product>();
            //for (var i = 0; i < 10; i++)
            //    products.Add(new Product {Id = i, Name = "Ball" + i, Category = "Sport" + i, Price = 500m + i});

            var pagesCount = GetPagesCount(products);

            var newPageIndex = GetNewPageIndex(id, pagesCount);

            var data = _tableManager.GetPageData(products, newPageIndex, PageSize).ToArray();
            var pager = new Pager
            {
                PageIndex = newPageIndex,
                PagesCount = pagesCount,
                PageSize = PageSize,
                PagerVisible = true,
                Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount)
            };

            return new Table<Product> {Data = data, Pager = pager};
        }

        private static int GetPagesCount(IReadOnlyCollection<Product> products)
        {
           return (int) Math.Ceiling((double) products.Count/PageSize);
        }

        private int GetNewPageIndex(string id, int pagesCount)
        {
            var oldPageIndex = (int) (_storageRepository.Get(Session, OldPageIndexName) ?? 1);
            var newPageIndex = _tableManager.GetPageIndex(id, oldPageIndex, pagesCount);
            _storageRepository.Set(Session, OldPageIndexName, newPageIndex);

            return newPageIndex;
        }
    }
}
