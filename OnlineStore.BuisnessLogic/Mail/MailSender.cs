using System;
using System.Collections.Generic;
using System.Net.Mail;
using OnlineStore.BuisnessLogic.Mail.Contracts;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.Mail
{
    public class MailSender : IMailSender
    {
        private readonly IMailService _mailService;

        private string _from;
        private string _to;
        private string _subject;
        private string _body;
        private bool _isBodyHtml;

        public MailSender(IMailService mailService)
        {
            _mailService = mailService;
        }

        public void Send()
        {
            if (!CheckIsMessageCreated()) throw new NullReferenceException();

            using (var mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(_from);
                mailMessage.To.Add(new MailAddress(_to));
                mailMessage.CC.Add(new MailAddress(_to));
                mailMessage.Subject = _subject;
                mailMessage.Body = _body;
                mailMessage.IsBodyHtml = _isBodyHtml;

                using (var client = new SmtpClient())
                {
                    client.Send(mailMessage);
                }
            }
        }

        public void Create(string @from, string to, string subject, IEnumerable<OrderItemDto> orderItemsBody,
            bool isBodyHtml, string ordersFormat, string bodyFormat, IFormatProvider cultureCurrency)
        {
            SuccessfulySend = false;

            _from = @from;
            _to = to;
            _subject = subject;
            _body = _mailService.GetBody(orderItemsBody, ordersFormat, bodyFormat, cultureCurrency);
            _isBodyHtml = isBodyHtml;
        }

        public bool CheckIsMessageCreated()
        {
            SuccessfulySend = false;

            return !string.IsNullOrEmpty(_from) && 
                   !string.IsNullOrEmpty(_to) && 
                   !string.IsNullOrEmpty(_subject) &&
                   !string.IsNullOrEmpty(_body);
        }

        public bool SuccessfulySend { get; private set; }
    }
}