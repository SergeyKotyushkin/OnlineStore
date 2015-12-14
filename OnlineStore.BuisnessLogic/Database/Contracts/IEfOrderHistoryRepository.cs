using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.Database.Contracts
{
    public interface IDbOrderHistoryRepository
    {
        IQueryable<OrderHistory> GetAll { get; }

        bool Add(OrderHistory orderHistory);

        bool Add(IEnumerable<OrderItemDto> orderItems, string userName, string userEmail, CultureInfo culture);
    }
}
