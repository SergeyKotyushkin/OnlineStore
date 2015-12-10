using System.Globalization;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Database.Models.Dto;

namespace OnlineStore.BuisnessLogic.MappingDto
{
    public class ProductDtoMapping
    {
        public static ProductDto ToDto(Product product, int orderCount, CultureInfo culture)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price.ToString("C", culture),
                OrderCount = orderCount
            };
        }
    }
}