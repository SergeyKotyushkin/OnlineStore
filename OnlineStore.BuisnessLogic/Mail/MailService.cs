using System;
using System.Collections.Generic;
using System.Linq;
using OnlineStore.BuisnessLogic.Mail.Contracts;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.Mail
{
    public class MailService : IMailService
    {
        public string GetBody(IEnumerable<OrderItem> orderItemsBody, string ordersFormat, string bodyFormat,
            IFormatProvider cultureCurrency)
        {
            var orderItems = orderItemsBody.ToArray();

            var orderList = string.Format("{0}</ul>", orderItems.Aggregate("<ul>",
                (current, p) =>
                    current + string.Format(ordersFormat, p.Name, p.Count, p.Price.ToString("C", cultureCurrency))));

            var total = orderItems.Sum(p => p.Total);

            return string.Format(bodyFormat, DateTime.Now.Date.ToShortDateString(), orderList,
                total.ToString("C", cultureCurrency));
        }
    }
}