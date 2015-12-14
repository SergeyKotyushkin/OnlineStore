using System.Web.Mvc;
using System.Web.Routing;

namespace OnlineStore.MvcWebProject
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{language}/{controller}/{action}/{id}",
                defaults: new { language = "en-US", controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { language = "[a-z]{2}[-][A-Z]{2}" }
            );
        }
    }
}