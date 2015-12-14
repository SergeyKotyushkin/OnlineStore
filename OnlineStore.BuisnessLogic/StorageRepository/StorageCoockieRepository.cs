using System.Web;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;

namespace OnlineStore.BuisnessLogic.StorageRepository
{
    public class StorageCoockieRepository : IStorageRepository<HttpCookieCollection>
    {
        public void Set(HttpCookieCollection repository, string name, object value)
        {
            repository.Set(new HttpCookie(name, (string)value));
        }

        public void Remove(HttpCookieCollection repository, string name)
        {
            repository.Remove(name);
        }

        public object Get(HttpCookieCollection repository, string name)
        {
            var cookie = repository.Get(name);
            return cookie == null ? null : cookie.Value;
        }
    }
}