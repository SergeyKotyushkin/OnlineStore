using System.ComponentModel.DataAnnotations;
using Resources;

namespace OnlineStore.MvcWebProject.Models.Index
{
    public class Credentials
    {
        [Required(ErrorMessageResourceType = typeof (Lang), ErrorMessageResourceName = "Credentials_LoginFieldIsRequired")]
        public string Login { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "Credentials_PasswordFieldIsRequired")]
        public string Password { get; set; }
    }
}