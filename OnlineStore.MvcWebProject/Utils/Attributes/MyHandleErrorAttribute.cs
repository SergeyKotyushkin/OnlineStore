using System;
using System.Drawing;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using log4net;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.MvcWebProject.Utils.Attributes
{
    public class MyHandleErrorAttribute : HandleErrorAttribute
    {
        private readonly Color _failColor = Color.Firebrick;
        private readonly ILog _log = LogManager.GetLogger(typeof(MyHandleErrorAttribute));

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        error = true,
                        message = new Message {Text = filterContext.Exception.Message, Color = _failColor}
                    }
                };
            }
            else
            {
                var controllerName = (string) filterContext.RouteData.Values["controller"];
                var actionName = (string) filterContext.RouteData.Values["action"];
                var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);

                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };

                filterContext.RequestContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.RequestContext.HttpContext.Response.Cache.SetExpires(DateTime.Now);

                FormsAuthentication.SignOut();
                if (filterContext.RequestContext.HttpContext.Session != null)
                    filterContext.RequestContext.HttpContext.Session.Abandon();
            }

            #region Log error

            var inner = filterContext.Exception;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }
            var message = filterContext.Exception.GetType() + " (" + filterContext.Exception.Source + ")\n" +
                          "Inner exception: " + inner.Message;
            _log.Error(string.Format("An {0} ({1}) occured.\nMessage: {2}", filterContext.Exception.GetType(),
                filterContext.Exception.Source, message));

            #endregion

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }

}