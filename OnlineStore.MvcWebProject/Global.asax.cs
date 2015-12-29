using System;
using System.Drawing;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using log4net;
using Newtonsoft.Json;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.StructureMap;
using OnlineStore.MvcWebProject.Controllers;
using OnlineStore.MvcWebProject.Utils.Attributes;
using Resources;

namespace OnlineStore.MvcWebProject
{
    public class MvcApplication : HttpApplication
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MvcApplication));

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

            #region Log error

            var inner = exception;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }
            var message = exception.GetType() + " (" + exception.Source + ")\n" +
                          "Inner exception: " + inner.Message;
            _log.Error(string.Format("An {0} ({1}) occured.\nMessage: {2}", exception.GetType(), exception.Source,
                message));

            #endregion

            Response.Clear();
            Server.ClearError();

            // if the request is AJAX return JSON else view.
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Response.StatusCode = 500;
                Response.Write(
                    JsonConvert.SerializeObject(
                        new {error = true, message = new Message {Text = Lang.CommonError, Color = Color.Firebrick}}));
                return;
            }
            
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

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now);

            FormsAuthentication.SignOut();
            Session.Abandon();

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