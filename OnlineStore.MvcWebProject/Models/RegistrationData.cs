using System.ComponentModel.DataAnnotations;
using Resources;

namespace OnlineStore.MvcWebProject.Models
{
    public class RegistrationData
    {
        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "LoginFieldIsRequired")]
        [RegularExpression("^[a-zA-Z]+[a-zA-Z0-9_]{4,}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "LoginRegexError")]
        public string Login { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordFieldIsRequired")]
        [RegularExpression("^[a-zA-Z0-9_!@#$%^&*]{5,}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordRegexError")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordFieldIsRequired")]
        [RegularExpression("^[a-zA-Z0-9_!@#$%^&*]{5,}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "PasswordRegexError")]
        public string PasswordRepeat { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "EmailFieldIsRequired")]
        [RegularExpression("^[a-zA-Z]+[a-zA-Z_0-9]{2,}@.{4,10}$", ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "EmailRegexError")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "QuestionFieldIsRequired")]
        public string Question { get; set; }

        [Required(ErrorMessageResourceType = typeof(Lang), ErrorMessageResourceName = "AnswerFieldIsRequired")]
        public string Answer { get; set; }
    }
}