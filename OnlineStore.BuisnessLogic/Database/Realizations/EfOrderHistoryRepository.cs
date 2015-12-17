using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.EfContexts;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.Database.Realizations
{
    public class EfOrderHistoryRepository : IDbOrderHistoryRepository
    {
        private readonly EfPersonContext _context = new EfPersonContext();

        public IQueryable<OrderHistory> GetAll()
        {
           return _context.OrdersHistoryTable;
        }

        public bool Add(OrderHistory orderHistory)
        {
            _context.OrdersHistoryTable.AddOrUpdate(orderHistory);
            return _context.SaveChanges() > 0;
        }

        public bool Add(IEnumerable<OrderItemDto> orderItems, string userName, string userEmail, CultureInfo culture)
        {
            var orderHistory = CreateOrderHistory(orderItems, userName, userEmail, culture);

            return Add(orderHistory);
        }

        private static OrderHistory CreateOrderHistory(IEnumerable<OrderItemDto> orderItems, string userName, string userEmail, CultureInfo culture)
        {
            var jsonSerialiser = new JavaScriptSerializer();
            var order = jsonSerialiser.Serialize(orderItems);

            return new OrderHistory
            {
                Order = order,
                PersonName = userName,
                PersonEmail = userEmail,
                Total = orderItems.Sum(p => decimal.Parse(p.Total, NumberStyles.Currency, culture)),
                Date = DateTime.Now,
                Culture = culture.Name
            };
        }
    }
}