using System.Web.Mvc;
using log4net;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;

namespace OnlineStore.MvcWebProject.Controllers
{
    public class MainLayoutController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HomeController));
        private readonly IUserGroup _userGroup;

        public MainLayoutController(IUserGroup userGroup)
        {
            _userGroup = userGroup;
        }

        [Authorize]
        public ActionResult LogOut()
        {
            var user = _userGroup.GetUser();

            Log.Error(string.Format("User {0} sing out.", user.UserName));

            _userGroup.LogOut(Response, Session);

            return RedirectToAction("Index", "Home");
        }

    }
}
