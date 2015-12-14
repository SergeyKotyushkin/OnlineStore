using System.Web;
using OnlineStore.BuisnessLogic.Lang.Contracts;

namespace OnlineStore.BuisnessLogic.Lang
{
    public class LangSetter : ILangSetter
    {
        public string Set(string name)
        {
            return (string)HttpContext.GetGlobalResourceObject("Lang", name);
        } 
    }
}