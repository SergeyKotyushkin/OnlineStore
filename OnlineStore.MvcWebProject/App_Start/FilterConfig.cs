using System.Web.Mvc;
using OnlineStore.MvcWebProject.Utils.Attributes;

namespace OnlineStore.MvcWebProject
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new MyHandleErrorAttribute());
        }
    }
}