using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OnlineStore.BuisnessLogic.Lang.Contracts;
using OnlineStore.BuisnessLogic.Mail.Contracts;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.Mail
{
    public class MailService : IMailService
    {
        private readonly ILangSetter _langSetter;

        public MailService(ILangSetter langSetter)
        {
            _langSetter = langSetter;
        }

        public string GetBody(IEnumerable<OrderItemDto> orderItemsBody, IFormatProvider cultureCurrency)
        {
            var orderItems = orderItemsBody.ToArray();

            var orderList = string.Format("{0}</ul>", orderItems.Aggregate("<ul>",
                (current, p) =>
                    current +
                    string.Format(
                        _langSetter.Set("Basket_MailOrderList"),
                        p.Name,
                        p.Count,
                        p.Price)));

            var total = orderItems.Sum(p => decimal.Parse(p.Total, NumberStyles.Currency, cultureCurrency));

            var mailMessageBody =
                string.Format(
                    _langSetter.Set("Basket_MailMessage"),
                    DateTime.Now.Date.ToShortDateString(),
                    orderList,
                    total.ToString("C", cultureCurrency));

            return mailMessageBody;
        }
    }
}