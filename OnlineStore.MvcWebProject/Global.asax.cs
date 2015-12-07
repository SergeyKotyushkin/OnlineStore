using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using OnlineStore.BuisnessLogic.StructureMap;

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
    }
}