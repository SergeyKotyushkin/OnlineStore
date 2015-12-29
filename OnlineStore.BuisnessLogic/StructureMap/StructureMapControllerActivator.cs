using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using StructureMap;

namespace OnlineStore.BuisnessLogic.StructureMap
{
    public class StructureMapControllerActivator : IControllerActivator
    {
        private readonly IContainer _container;

        public StructureMapControllerActivator()
        {
            _container = StructureMapFactory.GetContainer();
        }

        public IController Create(RequestContext requestContext, Type controllerType)
        {
            var language =
                (requestContext.RouteData.Values["language"] ?? Thread.CurrentThread.CurrentCulture.Name)
                    .ToString();

            var allowedLanguages = ConfigurationManager.AppSettings["Languages"].Split(',');
            if (!allowedLanguages.Contains(language))
                language = allowedLanguages[0];

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);

            return (IController)_container.GetInstance(controllerType);
        }
    }
}