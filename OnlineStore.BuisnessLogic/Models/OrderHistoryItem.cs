using System;

namespace OnlineStore.BuisnessLogic.Models
{
    public class OrderHistoryItem
    {
        public int Number;
        public DateTime Date;
        public string Email;
        public ProductOrder[] ProductOrder;
        public decimal Total;
        public string CultureName;
    }
}