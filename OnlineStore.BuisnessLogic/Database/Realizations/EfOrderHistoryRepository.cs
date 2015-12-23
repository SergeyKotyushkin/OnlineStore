using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.EfContexts;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.Database.Realizations
{
    public class EfOrderHistoryRepository : IDbOrderHistoryRepository
    {
        public List<OrderHistory> GetAll()
        {
            using (var context = new EfPersonContext())
            {
                return context.OrdersHistoryTable.ToList();
            }
        }

        public bool Add(OrderHistory orderHistory)
        {
            using (var context = new EfPersonContext())
            {
                context.OrdersHistoryTable.AddOrUpdate(orderHistory);
                return context.SaveChanges() > 0;
            }
        }

        public bool Add(IEnumerable<OrderItem> orderItems, string userName, string userEmail, CultureInfo culture)
        {
            var orderHistory = CreateOrderHistory(orderItems, userName, userEmail, culture);
            return Add(orderHistory);
        }

        private static OrderHistory CreateOrderHistory(IEnumerable<OrderItem> orderItems, string userName,
            string userEmail, CultureInfo culture)
        {
            var jsonSerialiser = new JavaScriptSerializer();
            var order = jsonSerialiser.Serialize(orderItems);

            return new OrderHistory
            {
                Order = order,
                PersonName = userName,
                PersonEmail = userEmail,
                Total = orderItems.Sum(p => p.Total),
                Date = DateTime.Now,
                Culture = culture.Name
            };
        }
    }
}