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
using OnlineStore.BuisnessLogic.MappingDtoExtensions;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;
using OnlineStore.BuisnessLogic.OrderRepository.Contracts;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;
using OnlineStore.MvcWebProject.Models.Basket;
using OnlineStore.MvcWebProject.Utils.Attributes;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    [MyHandleError]
    [OnlyForRole("User")]
    public class BasketController : Controller
    {
        private const int PageSize = 8;
        private const int VisiblePagesCount = 5;

        private readonly PagerSettings _pagerSettings = new PagerSettings
        {
            PageChangeRoute = new Route {Action = "PageChange", Controller = "Basket"},
            UpdateTargetId = "basketTablePartial"
        };

        private readonly CultureInfo _threadCulture = Thread.CurrentThread.CurrentCulture;

        private static readonly ILog Log = LogManager.GetLogger(typeof(BasketController));

        private readonly IStorageRepository<HttpCookieCollection> _storageCookieRepository;
        private readonly IDbProductRepository _dbProductRepository;
        private readonly IOrderRepository<HttpSessionStateBase> _orderRepository;
        private readonly ICurrencyConverter _currencyConverter;
        private readonly ITableManager<OrderItemDto, HttpSessionStateBase> _tableManager;
        private readonly IUserGroup _userGroup;
        private readonly IDbOrderHistoryRepository _dbOrderHistoryRepository;
        private readonly IMailSender _mailSender;

        public BasketController(IStorageRepository<HttpCookieCollection> storageCookieRepository,
            IDbProductRepository dbProductRepository, IOrderRepository<HttpSessionStateBase> orderRepository,
            ICurrencyConverter currencyConverter, ITableManager<OrderItemDto, HttpSessionStateBase> tableManager, 
            IUserGroup userGroup, IDbOrderHistoryRepository dbOrderHistoryRepository, IMailSender mailSender)
        {
            _storageCookieRepository = storageCookieRepository;
            _dbProductRepository = dbProductRepository;
            _orderRepository = orderRepository;
            _currencyConverter = currencyConverter;
            _tableManager = tableManager;
            _userGroup = userGroup;
            _dbOrderHistoryRepository = dbOrderHistoryRepository;
            _mailSender = mailSender;
        }

        public ActionResult Orders()
        {
            var id = _tableManager.GetOldPageIndexFromRepository(Session, Settings.Basket_OldPageIndexName);
            var currencyCulture = GetCurrencyCultureInfo();

            var model = new BasketModel
            {
                TableData = GetTableData(id),
                Total = GetTotalString(currencyCulture),
                Settings = new MainLayoutSettings
                {
                    Title = Lang.Basket_Title,
                    MoneyVisible = true,
                    LinkProfileText = string.Format(Lang.MainLayout_LinkProfileText, _userGroup.GetUser().UserName),
                    LogoutVisible = true,
                    BackVisible = true,
                    SelectedLanguage = _threadCulture.Name,
                    SelectedCurrency = currencyCulture.Name
                }
            };

            return View(model);
        }
        
        public PartialViewResult PageChange(int pageindex)
        {
            var table = GetTableData(pageindex);

            return PartialView("_basketTable", table);
        }

        public ActionResult Buy()
        {
            var user = _userGroup.GetUser();

            var currencyCulture = GetCurrencyCultureInfo();

            var orderItemList = GetOrderItemList(0, true).ToArray();

            SaveOrderHistoryInDatabase(orderItemList, user.UserName, user.Email, currencyCulture);

            SendMailMessage(user.Email, orderItemList);

            MakePurchase(user.UserName);

            return RedirectToAction("Products", "Catalog");
        }


        private Table<OrderItemDto> GetTableData(int pageIndex)
        {
            var ordersCount = _orderRepository.GetCount(Session, Settings.OrderInStorage);

            var pagesCount = _tableManager.GetPagesCount(ordersCount, PageSize);
            
            var oldPageIndex = _tableManager.GetOldPageIndexFromRepository(Session, Settings.Basket_OldPageIndexName);
            var newPageIndex = _tableManager.GetNewPageIndex(pageIndex, oldPageIndex, pagesCount);
            _tableManager.SetNewPageIndexToRepository(Session, Settings.Basket_OldPageIndexName, newPageIndex);

            var orderItemDtos = GetOrderItemDtoList(newPageIndex - 1, false);

            var pager = new Pager
            {
                PageIndex = newPageIndex,
                PagesCount = pagesCount,
                PageSize = PageSize,
                Pages = _tableManager.GetPages(newPageIndex, pagesCount, VisiblePagesCount),
                PagerSettings = _pagerSettings
            };

            return new Table<OrderItemDto> { Data = orderItemDtos, Pager = pager };
        }

        private OrderItemDto[] GetOrderItemDtoList(int pageIndex, bool allOrders)
        {
            var orderItemArray = GetOrderItemList(pageIndex, allOrders);

            var currencyCulture = GetCurrencyCultureInfo();

            return orderItemArray.Select(ot => ot.ToOrderItemDto(currencyCulture)).ToArray();
        }

        private IEnumerable<OrderItem> GetOrderItemList(int pageIndex, bool allOrders)
        {
            var orders = allOrders
                ? _orderRepository.GetAll(Session, Settings.OrderInStorage).ToArray()
                : _orderRepository.GetRange(Session, Settings.OrderInStorage, pageIndex, PageSize);
            
            var productsId = orders.Select(o => o.Id).ToArray();
            var products = _dbProductRepository.GetByIds(productsId);

            var currencyCulture = GetCurrencyCultureInfo();
            var rate = _currencyConverter.GetRate(new CultureInfo("ru-Ru"), currencyCulture, DateTime.Now);

            return products.Join(orders, p => p.Id, q => q.Id, (p, q) => new OrderItem
            {
                Name = p.Name,
                Price = _currencyConverter.ConvertByRate(p.Price, rate),
                Count = q.Count,
                Total = _currencyConverter.ConvertByRate(p.Price, rate)*q.Count
            });
        }

        private decimal GetTotal(IFormatProvider culture)
        {
            return GetOrderItemDtoList(0, true).Sum(t => decimal.Parse(t.Total, NumberStyles.Currency, culture));
        }
        
        private string GetTotalString(IFormatProvider currencyCulture)
        {
            return Lang.Basket_Total + " " + GetTotal(currencyCulture).ToString("C", currencyCulture);
        }
        
        private void SaveOrderHistoryInDatabase(IEnumerable<OrderItem> orderItemList, string userName, string userEmail,
            CultureInfo culture)
        {
            _dbOrderHistoryRepository.Add(orderItemList, userName, userEmail, culture);
        }

        private void MakePurchase(string userName)
        {
            var currencyCulture = GetCurrencyCultureInfo();
            Log.Info(string.Format("Products has bought by user - {0}. {1}", userName, GetTotalString(currencyCulture)));
            Session[Settings.BoughtInStorage] = true;
            Session[Settings.OrderInStorage] = null;
        }

        private void SendMailMessage(string userEmail, IEnumerable<OrderItem> orderItemList)
        {
            var @from = ((SmtpSection)ConfigurationManager.GetSection(Settings.Basket_SmtpSectionPath)).From;
            var mailMessageSubject = Lang.Basket_MailMessageSubject;

            var cultureCurrency = GetCurrencyCultureInfo();

            _mailSender.Create(@from, userEmail, mailMessageSubject, orderItemList, true, Lang.Basket_MailOrderList,
                Lang.Basket_MailMessage, cultureCurrency);
            _mailSender.Send();
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
