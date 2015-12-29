using System.ComponentModel.DataAnnotations;
using Resources;

namespace OnlineStore.MvcWebProject.Models.Home
{
    public class Credentials
    {
        [Required(ErrorMessageResourceType = typeof (Lang), ErrorMessageResourceName = "LoginFieldIsRequired")]
        public string Login { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordFieldIsRequired")]
        public string Password{ get; set; }
    }
}