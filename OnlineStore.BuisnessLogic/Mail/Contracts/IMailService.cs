using System;
using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.Mail.Contracts
{
    public interface IMailService
    {
        string GetBody(IEnumerable<OrderItem> orderItemsBody, string ordersFormat, string bodyFormat,
            IFormatProvider cultureCurrency);
    }
}