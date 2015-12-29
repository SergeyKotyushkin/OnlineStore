using System.Collections.Generic;
using System.Globalization;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.Database.Contracts
{
    public interface IDbOrderHistoryRepository
    {
        OrderHistory[] GetRange(int from, int size, string userName);

        int GetCount();

        bool Add(OrderHistory orderHistory);

        bool Add(IEnumerable<OrderItem> orderItems, string userName, string userEmail, CultureInfo culture);
    }
}
