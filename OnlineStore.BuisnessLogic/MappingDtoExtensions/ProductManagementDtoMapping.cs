using System;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.MappingDtoExtensions
{
    public static class ProductManagementDtoMapping
    {
        public static ProductManagementDto ToProductManagementDto(this Product product, IFormatProvider culture)
        {
            return new ProductManagementDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price.ToString("C", culture)
            };
        }
    }
}