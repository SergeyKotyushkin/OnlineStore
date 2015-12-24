using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using OnlineStore.BuisnessLogic.StructureMap;
using OnlineStore.MvcWebProject.Controllers;

namespace OnlineStore.MvcWebProject
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            StructureMapFactory.Init();
            var container = StructureMapFactory.GetContainer();
            container.Configure(x => x.For<IControllerActivator>().Use<StructureMapControllerActivator>());

            DependencyResolver.SetResolver(new StructureMapDependencyResolver(container));
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;
            Response.Clear();
            Server.ClearError();

            var action = "UnhandledServerError";
            if (httpException != null)
            {
                switch (httpException.GetHttpCode())
                {
                    case 400:
                        action = "BadRequest";
                        break;
                    case 401:
                        action = "Unauthorized";
                        break;
                    case 403:
                        action = "Forbidden";
                        break;
                    case 404:
                        action = "NotFound";
                        break;
                    case 408:
                        action = "RequestTimeout";
                        break;
                    case 500:
                        action = "InternalServerError";
                        break;
                    default:
                        action = "InternalServerError";
                        break;
                }
            }

                var routeData = new RouteData();
                routeData.Values["controller"] = "Errors";
                routeData.Values["action"] = action;
                routeData.Values["language"] = Request.RequestContext.RouteData.Values["language"];

                var requestContext = new RequestContext(new HttpContextWrapper(Context), routeData);
                var controller = new StructureMapControllerActivator().Create(requestContext,
                    typeof (ErrorsController));
                controller.Execute(requestContext);
            
        }
    }
}