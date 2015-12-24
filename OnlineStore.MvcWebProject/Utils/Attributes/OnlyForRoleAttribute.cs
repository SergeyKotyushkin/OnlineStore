using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OnlineStore.MvcWebProject.Utils.Attributes
{
    public class OnlyForRoleAttribute : AuthorizeAttribute
    {
        private readonly string _role;

        public OnlyForRoleAttribute(string role)
        {
            _role = role;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.User.IsInRole(_role);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                language = Thread.CurrentThread.CurrentCulture.Name,
                                controller = "Home",
                                action = "Index"
                            }));
            }
        }
    }
}