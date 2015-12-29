using System.Web;
using System.Web.Mvc;
using log4net;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;
using OnlineStore.MvcWebProject.Utils.Attributes;

namespace OnlineStore.MvcWebProject.Controllers
{
    [MyHandleError]
    public class MainLayoutController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainLayoutController));
        private readonly IUserGroup _userGroup;
        private readonly IStorageRepository<HttpCookieCollection> _storageCookieRepository;

        public MainLayoutController(IUserGroup userGroup,
            IStorageRepository<HttpCookieCollection> storageCookieRepository)
        {
            _userGroup = userGroup;
            _storageCookieRepository = storageCookieRepository;
        }

        [OnlyForAuthenticated]
        public ActionResult LogOut()
        {
            var user = _userGroup.GetUser();

            Log.Info(string.Format("User {0} sing out.", user.UserName));

            _userGroup.LogOut(Response, Session);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ChangeLanguage(string language)
        {
            return RedirectBackByUrlReferrer(new {language});
        }

        public ActionResult ChangeCurrency(string currency)
        {
            _storageCookieRepository.Set(Response.Cookies, Settings.CurrencyInStorage, currency);
            return RedirectBackByUrlReferrer(new {});
        }


        private ActionResult RedirectBackByUrlReferrer(object routeValues)
        {
            if (Request.UrlReferrer == null || Request.UrlReferrer.Segments.Length < 3)
                return RedirectToAction("Index", "Home", routeValues);

            var urlSegments = Request.UrlReferrer.Segments;
            var action = Request.UrlReferrer.Segments.Length == 3 ? "Index" : urlSegments[3];
            var controller = Request.UrlReferrer.Segments.Length == 3
                ? urlSegments[2]
                : urlSegments[2].Substring(0, urlSegments[2].Length - 1);

            return RedirectToAction(action, controller, routeValues);
        }
    }
}
