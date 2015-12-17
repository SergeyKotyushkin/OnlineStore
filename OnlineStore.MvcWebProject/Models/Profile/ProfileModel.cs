using OnlineStore.BuisnessLogic.Database.Models.Dto;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.MvcWebProject.Models.Profile
{
    public class ProfileModel : ViewModelBase
    {
        public PersonDto UserProfile { get; set; }

        public Password Password { get; set; }

        public Table<OrderHistoryItemDto> TableData { get; set; } 
        
        public override MainLayoutSettings Settings { get; set; }

        public override Message Message { get; set; }
    }
}