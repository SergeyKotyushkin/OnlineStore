using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.MvcWebProject.Models.ProductCatalog
{
    public class ProductCatalogModel : ViewModelBase
    {
        public Table<ProductDto> TableData { get; set; }

        public Search Search { get; set; }

        public override MainLayoutSettings Settings { get; set; }

        public override Message Message { get; set; }
    }
}