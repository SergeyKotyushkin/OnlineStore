using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OnlineStore.BuisnessLogic.Mail.Contracts;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.Mail
{
    public class MailService : IMailService
    {
        public string GetBody(IEnumerable<OrderItemDto> orderItemsBody, string ordersFormat, string bodyFormat, IFormatProvider cultureCurrency)
        {
            var orderItems = orderItemsBody.ToArray();

            var orderList = string.Format("{0}</ul>", orderItems.Aggregate("<ul>",
                (current, p) => current + string.Format(ordersFormat, p.Name, p.Count, p.Price)));

            var total = orderItems.Sum(p => decimal.Parse(p.Total, NumberStyles.Currency, cultureCurrency));

            var mailMessageBody =
                string.Format(
                    bodyFormat,
                    DateTime.Now.Date.ToShortDateString(),
                    orderList,
                    total.ToString("C", cultureCurrency));

            return mailMessageBody;
        }
    }
}