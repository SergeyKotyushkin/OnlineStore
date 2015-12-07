using StructureMap;

namespace OnlineStore.BuisnessLogic.StructureMap
{
    public class StructureMapFactory
    {
        private static IContainer _container;
        
        public static void Init()
        {
            _container = CreateContainer();
        }

        public static IContainer GetContainer()
        {
            return _container ?? (_container = CreateContainer());
        }

        public static T Resolve<T>()
        {
            return _container.GetInstance<T>();
        }

        private static IContainer CreateContainer()
        {
            var container = new Container();
            container.Configure(c =>
            {
                c.IncludeRegistry<AllRegistry>();
            });

            return container;
        }
    }
}