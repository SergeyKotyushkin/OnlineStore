using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using log4net;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;
using OnlineStore.MvcWebProject.App_GlobalResources;

namespace OnlineStore.MvcWebProject.Controllers
{
    public class MainLayoutController : Controller
    {
        private readonly string[] _languages = { "ru-RU", "en-US" };
        private static readonly ILog Log = LogManager.GetLogger(typeof(HomeController));
        private readonly IUserGroup _userGroup;
        private readonly IStorageRepository<HttpCookieCollection> _storageCookieRepository;

        public MainLayoutController(IUserGroup userGroup,
            IStorageRepository<HttpCookieCollection> storageCookieRepository)
        {
            _userGroup = userGroup;
            _storageCookieRepository = storageCookieRepository;
        }

        [Authorize]
        public ActionResult LogOut()
        {
            var user = _userGroup.GetUser();

            Log.Error(string.Format("User {0} sing out.", user.UserName));

            _userGroup.LogOut(Response, Session);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ChangeLanguage(string language)
        {
            if (!_languages.Contains(language))
                language = Thread.CurrentThread.CurrentUICulture.Name;

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
