using System.Globalization;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.MappingDto
{
    public class OrderItemDtoMapping
    {
        public static OrderItemDto ToDto(OrderItem orderItem, CultureInfo culture)
        {
            return new OrderItemDto
            {
                Name = orderItem.Name,
                Price = orderItem.Price.ToString("C", culture),
                Count = orderItem.Count,
                Total = orderItem.Total.ToString("C", culture)
            };
        } 
    }
}