using System;
using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.Mail.Contracts
{
    public interface IMailSender
    {
        void Send();

        void Create(string @from, string to, string subject, IEnumerable<OrderItem> orderItemsBody, bool isBodyHtml,
            string ordersFormat, string bodyFormat, IFormatProvider cultureCurrency);

        bool CheckIsMessageCreated();
    }
}