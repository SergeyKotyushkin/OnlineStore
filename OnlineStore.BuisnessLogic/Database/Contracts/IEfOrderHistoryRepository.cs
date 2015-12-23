using System.Collections.Generic;
using System.Globalization;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.Database.Contracts
{
    public interface IDbOrderHistoryRepository
    {
        List<OrderHistory> GetAll();

        bool Add(OrderHistory orderHistory);

        bool Add(IEnumerable<OrderItem> orderItems, string userName, string userEmail, CultureInfo culture);
    }
}
