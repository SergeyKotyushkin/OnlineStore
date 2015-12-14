using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Configuration;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using log4net;
using OnlineStore.BuisnessLogic.Currency.Contracts;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Mail.Contracts;
using OnlineStore.BuisnessLogic.MappingDto;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;
using OnlineStore.BuisnessLogic.OrderRepository.Contracts;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.Models.Basket;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    public class BasketController : Controller
    {
        private const int PageSize = 8;
        private const int VisiblePagesCount = 5;
        private const string OrderInStorage = "CurrentOrder";
        private const string BoughtInStorage = "Bought";
        private const string OldPageIndexName = "CurrentPageIndexB";
        private const string SmtpSectionPath = "system.net/mailSettings/smtp";

        private static readonly ILog Log = LogManager.GetLogger(typeof(BasketController));

        private readonly IStorageRepository<HttpCookieCollection> _storageCookieRepository;
        private readonly IStorageRepository<HttpSessionStateBase> _storageSessionRepository;
        private readonly IDbProductRepository _dbProductRepository;
        private readonly IOrderRepository<HttpSessionStateBase> _orderRepository;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly ITableManager<OrderItemDto> _tableManager;
        private readonly IUserGroup _userGroup;
        private readonly IDbOrderHistoryRepository _dbOrderHistoryRepository;
        private readonly IMailSender _mailSender;

        public BasketController(IStorageRepository<HttpCookieCollection> storageCookieRepository,
            IDbProductRepository dbProductRepository, IOrderRepository<HttpSessionStateBase> orderRepository,
            ICurrencyConverter currencyConverter, IStorageRepository<HttpSessionStateBase> storageSessionRepository,
            ITableManager<OrderItemDto> tableManager, IUserGroup userGroup,
            IDbOrderHistoryRepository dbOrderHistoryRepository, IMailSender mailSender)
        {
            _storageCookieRepository = storageCookieRepository;
            _dbProductRepository = dbProductRepository;
            _orderRepository = orderRepository;
            _currencyConverter = currencyConverter;
            _storageSessionRepository = storageSessionRepository;
            _tableManager = tableManager;
            _userGroup = userGroup;
            _dbOrderHistoryRepository = dbOrderHistoryRepository;
            _mailSender = mailSender;
        }

        public ActionResult AllOrders(string id)
        {
            var model = new BasketModel
            {
                TableData = GetTableData(id),
                Total = GetTotal(),
                Settings = new MainLayoutSettings
                {
                    Title = Lang.Basket_Title,
                    MoneyVisible = true,
                    ProfileVisible = true,
                    LogoutVisible = true,
                    SelectedLanguage = Thread.CurrentThread.CurrentCulture.Name,
                    SelectedCurrency = (_storageCookieRepository.Get(Request.Cookies, Lang.CurrencyInStorage) ??
                                        Thread.CurrentThread.CurrentUICulture.Name).ToString(),
                    RouteBack = new Route {Action = "ProductList", Controller = "ProductCatalog"}
                }
            };

            return View(model);
        }

        public PartialViewResult PageChange(int pageindex)
        {
            var table = GetTableData(pageindex.ToString());

            return PartialView("_basketAllOrdersTable", table);
        }

        public ActionResult Buy()
        {
            var user = _userGroup.GetUser();

            var cultureName = (_storageCookieRepository.Get(Request.Cookies, Lang.CurrencyInStorage) ??
                            Thread.CurrentThread.CurrentUICulture.Name).ToString();
            var culture = CultureInfo.GetCultureInfo(cultureName);

            var orderItemList = GetOrderItemsList();

            SaveOrderHistoryInDatabase(orderItemList, user.UserName, user.Email, culture);

            SendMailMessage(user.Email, orderItemList);

            MakePurchase(user.UserName);

            return RedirectToAction("ProductList", "ProductCatalog");
        }


        private Table<OrderItemDto> GetTableData(string id)
        {
            var orderItemDtos = GetOrderItemsList();

            var pagesCount = GetPagesCount(orderItemDtos);

            var newPageIndex = GetNewPageIndex(id, pagesCount);

            var data = _tableManager.GetPageData(orderItemDtos, newPageIndex, PageSize).ToArray();
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
                        PageChangeRoute = new Route{Action = "PageChange", Controller = "Basket"},
                        UpdateTargetId = "basketTablePartial"
                    }
            };

            return new Table<OrderItemDto> { Data = data, Pager = pager };
        }

        private List<OrderItemDto> GetOrderItemsList()
        {
            var products = _dbProductRepository.GetAll();
            var orders = _orderRepository.GetAll(Session, OrderInStorage);

            var currencyCultureName =
                (_storageCookieRepository.Get(Request.Cookies, Lang.CurrencyInStorage) ??
                 Thread.CurrentThread.CurrentUICulture.Name).ToString();
            var culture = CultureInfo.GetCultureInfo(currencyCultureName);
            var rate = _currencyConverter.GetRate(new CultureInfo("ru-Ru"), culture, DateTime.Now);

            foreach (var p in products.Where(p => orders.Select(o => o.Id).Contains(p.Id)))
                p.Price = _currencyConverter.ConvertByRate(p.Price, rate);

            var orderItemArray = products.Join(orders, p => p.Id, q => q.Id, (p, q) => new OrderItem
            {
                Name = p.Name,
                Price = p.Price,
                Count = q.Count,
                Total = q.Count * p.Price
            }).ToArray();

            return orderItemArray.Select(ot => OrderItemDtoMapping.ToDto(ot, culture)).ToList();
        }

        private string GetTotal()
        {
            var products = _dbProductRepository.GetAll();
            var orders = _orderRepository.GetAll(Session, OrderInStorage);

            var currencyCultureName =
                (_storageCookieRepository.Get(Request.Cookies, Lang.CurrencyInStorage) ??
                 Thread.CurrentThread.CurrentUICulture.Name).ToString();
            var culture = CultureInfo.GetCultureInfo(currencyCultureName);
            var rate = _currencyConverter.GetRate(new CultureInfo("ru-Ru"), culture, DateTime.Now);

            var total =
                products.Join(orders, p => p.Id, q => q.Id,
                    (p, q) => _currencyConverter.ConvertByRate(p.Price*q.Count, rate)).Sum();

            return Lang.Basket_Total + " " + total.ToString("C", culture);
        }

        private static int GetPagesCount(IReadOnlyCollection<OrderItemDto> products)
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

        private void SaveOrderHistoryInDatabase(IEnumerable<OrderItemDto> orderItemList, string userName, string userEmail,
            CultureInfo culture)
        {
            _dbOrderHistoryRepository.Add(orderItemList, userName, userEmail, culture);
        }

        private void MakePurchase(string userName)
        {
            Log.Info(string.Format("Products has bought by user - {0}. {1}", userName, GetTotal()));
            Session[BoughtInStorage] = 1;
            Session[OrderInStorage] = null;
        }

        private void SendMailMessage(string userEmail, IEnumerable<OrderItemDto> orderItemList)
        {
            var @from = ((SmtpSection)ConfigurationManager.GetSection(SmtpSectionPath)).From;
            var mailMessageSubject = Lang.Basket_MailMessageSubject;

            var currencyCultureName =
                (_storageCookieRepository.Get(Request.Cookies, Lang.CurrencyInStorage) ??
                 Thread.CurrentThread.CurrentUICulture.Name).ToString();
            var cultureCurrency = CultureInfo.GetCultureInfo(currencyCultureName);

            _mailSender.Create(@from, userEmail, mailMessageSubject, orderItemList, true, Lang.Basket_MailOrderList,
                Lang.Basket_MailMessage, cultureCurrency);
            _mailSender.Send();
        }
    }
}
