using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.MvcWebProject.Models.Registration
{
    public class RegistrationModel : ViewModelBase
    {
        public RegistrationData RegistrationData { get; set; }

        public override MainLayoutSettings Settings { get; set; }

        public override Message Message { get; set; }
    }
}