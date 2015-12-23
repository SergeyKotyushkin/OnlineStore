using System.ComponentModel.DataAnnotations;
using Resources;

namespace OnlineStore.MvcWebProject.Models.Index
{
    public class Credentials
    {
        [Required(ErrorMessageResourceType = typeof (Lang), ErrorMessageResourceName = "LoginFieldIsRequired")]
        [RegularExpression("^[a-zA-Z]+[a-zA-Z0-9_]{4,}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "LoginRegexError")]
        public string Login { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordFieldIsRequired")]
        [RegularExpression("^[a-zA-Z0-9_!@#$%^&*]{5,}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordRegexError")]
        public string Password{ get; set; }
    }
}