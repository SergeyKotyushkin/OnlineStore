using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.MvcWebProject.Models.Home
{
    public class IndexModel : ViewModelBase
    {
        public Credentials Credentials { get; set; }

        public override MainLayoutSettings Settings { get; set; }

        public override Message Message { get; set; }
    }
}