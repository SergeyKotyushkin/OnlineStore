using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.MvcWebProject.Models
{
    public class ManagementModel : ViewModelBase
    {
        public Table<ProductManagementDto> TableData { get; set; } 

        public override MainLayoutSettings Settings { get; set; }

        public override Message Message { get; set; }
    }
}