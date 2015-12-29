using System.Drawing;
using System.Threading;
using System.Web.Mvc;
using log4net;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;
using OnlineStore.MvcWebProject.Models.Registration;
using OnlineStore.MvcWebProject.Utils.Attributes;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    [MyHandleError]
    [OnlyForAnonymous]
    public class RegistrationController : Controller
    {
        private readonly Color _failColor = Color.Firebrick;
        
        private readonly MainLayoutSettings _mainLayoutSettings = new MainLayoutSettings
        {
            Title = Lang.Registration_Title,
            BackVisible = true,
            SelectedLanguage = Thread.CurrentThread.CurrentCulture.Name
        };

        private static readonly ILog Log = LogManager.GetLogger(typeof(RegistrationController));

        private readonly IUserGroup _userGroup;
        
        public RegistrationController(IUserGroup userGroup)
        {
            _userGroup = userGroup;
        }

        public ActionResult Index()
        {
            ModelState.Clear();

            return View(new RegistrationModel {Settings = _mainLayoutSettings});
        }

        [HttpPost]
        public ActionResult Index(RegistrationModel model)
        {
            if (!ModelState.IsValid || model.RegistrationData == null)
            {
                model.Settings = _mainLayoutSettings;
                return View(model);
            }

            if (_userGroup.CreateUser(model.RegistrationData.Login, model.RegistrationData.Password,
                model.RegistrationData.Email, model.RegistrationData.Question, model.RegistrationData.Answer))
            {
                Log.Info(string.Format("User {0} successfully created.", model.RegistrationData.Login));
                Session[Settings.NewUser] = true;
                return RedirectToAction("Index", "Home");
            }

            Log.Debug(string.Format("User {0} didn't create.", model.RegistrationData.Login));
            model.Message = new Message {Color = _failColor, Text = Lang.Registration_CreateUserError};
            model.Settings = _mainLayoutSettings;
            return View("Index", model);
            
        }
    }
}
