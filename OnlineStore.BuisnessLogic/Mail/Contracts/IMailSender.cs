using System;
using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.Mail.Contracts
{
    public interface IMailSender
    {
        void Send();

        void Create(string @from, string to, string subject, IEnumerable<OrderItemDto> orderItemsBody, bool isBodyHtml,
            IFormatProvider cultureCurrency);

        bool CheckIsMessageCreated();
    }
}