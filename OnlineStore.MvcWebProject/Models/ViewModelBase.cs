using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.MvcWebProject.Models
{
    public abstract class ViewModelBase
    {
        public abstract MainLayoutSettings Settings { get; set; }

        public abstract Message Message { get; set; }
    }
}