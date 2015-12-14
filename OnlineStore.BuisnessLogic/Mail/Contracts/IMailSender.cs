using System;
using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.Mail.Contracts
{
    public interface IMailSender
    {
        void Send();

        void Create(string @from, string to, string subject, IEnumerable<OrderItemDto> orderItemsBody, bool isBodyHtml,
            string ordersFormat, string bodyFormat, IFormatProvider cultureCurrency);

        bool CheckIsMessageCreated();
    }
}