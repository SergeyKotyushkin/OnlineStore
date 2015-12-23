using System.ComponentModel.DataAnnotations;
using Resources;

namespace OnlineStore.MvcWebProject.Models
{
    public class Password
    {
        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordFieldIsRequired")]
        [RegularExpression("^[a-zA-Z0-9_!@#$%^&*]{5,}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordRegexError")]
        public string Old { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordFieldIsRequired")]
        [RegularExpression("^[a-zA-Z0-9_!@#$%^&*]{5,}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordRegexError")]
        public string New { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordFieldIsRequired")]
        [RegularExpression("^[a-zA-Z0-9_!@#$%^&*]{5,}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordRegexError")]
        public string Repeat { get; set; }
    }
}