using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.MvcWebProject.Models.Basket
{
    public class BasketModel : ViewModelBase
    {
        public Table<OrderItemDto> TableData { get; set; }

        public string Total { get; set; }

        public override MainLayoutSettings Settings { get; set; }

        public override Message Message { get; set; }
    }
}