using System.Drawing;
using System.Threading;
using System.Web.Mvc;
using System.Web.Security;
using log4net;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.Models.Index;
using Resources;

namespace OnlineStore.MvcWebProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly Color _validateUserFail = Color.FromArgb(0xdc, 0x14, 0x3c);

        private readonly MainLayoutSettings _mainLayoutSettings = new MainLayoutSettings
        {
            Title = Lang.Index_Title,
            SelectedLanguage = Thread.CurrentThread.CurrentCulture.Name
        };

        private static readonly ILog Log = LogManager.GetLogger(typeof(HomeController));
        private readonly IUserGroup _userGroup;

        public HomeController(IUserGroup userGroup)
        {
            _userGroup = userGroup;
        }


        [HttpGet]
        public ActionResult Index()
        {
            var user = _userGroup.GetUser(true);

            return user == null
                ? (ActionResult) View(new IndexModel {Settings = _mainLayoutSettings})
                : RedirectFromIndexByRole(user.UserName);
        }

        [HttpPost]
        public ActionResult Index(IndexModel model)
        {
            if (!ModelState.IsValid)
                return View();

            ModelState.Clear();
            if (_userGroup.ValidateUser(model.Credentials.Login, model.Credentials.Password))
            {
                var login = model.Credentials.Login;

                Log.Info(string.Format("User {0} logged in.", login));
                FormsAuthentication.SetAuthCookie(login, false);
                return RedirectFromIndexByRole(login);
            }

            model.Message = new Message {Text = Lang.Index_ValidateUserFail, Color = _validateUserFail};
            model.Settings = _mainLayoutSettings;
            return View(model);
        }


        private RedirectToRouteResult RedirectFromIndexByRole(string userName)
        {
            return _userGroup.CheckIsUserIsAdmin(userName)
                ? RedirectToAction("ProductList", "ProductCatalog")
                : RedirectToAction("ProductList", "ProductCatalog");
        }
    }
}
