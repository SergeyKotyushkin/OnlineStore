using System;
using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.Mail.Contracts
{
    public interface IMailService
    {
        string GetBody(IEnumerable<OrderItemDto> orderItemsBody, string ordersFormat, string bodyFormat,
            IFormatProvider cultureCurrency);
    }
}