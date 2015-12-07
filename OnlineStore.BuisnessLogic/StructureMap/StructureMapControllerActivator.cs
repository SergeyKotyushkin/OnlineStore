using System;
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
            return (IController)_container.GetInstance(controllerType);
        }
    }
}