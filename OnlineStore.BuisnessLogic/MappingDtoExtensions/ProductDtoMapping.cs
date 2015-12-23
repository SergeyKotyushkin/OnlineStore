using System.Globalization;
using OnlineStore.BuisnessLogic.Currency.Contracts;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Database.Models.Dto;

namespace OnlineStore.BuisnessLogic.MappingDtoExtensions
{
    public static class ProductDtoMapping
    {
        public static ProductDto ToProductDto(this Product product, int orderCount, ICurrencyConverter currencyConverter,
            CultureInfo culture, decimal rate)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category,
                Price = currencyConverter.ConvertByRate(product.Price, rate).ToString("C", culture),
                OrderCount = orderCount
            };
        }
    }
}