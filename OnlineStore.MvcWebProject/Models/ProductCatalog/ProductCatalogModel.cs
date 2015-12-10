using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.MvcWebProject.Models.ProductCatalog
{
    public class ProductCatalogModel
    {
        public Table<ProductDto> TableData { get; set; }

        public Search Search { get; set; }

        public Message Message { get; set; }
    }
}