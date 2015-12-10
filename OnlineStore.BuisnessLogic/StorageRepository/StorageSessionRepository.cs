using System.Web;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;

namespace OnlineStore.BuisnessLogic.StorageRepository
{
    public class StorageSessionRepository : IStorageRepository<HttpSessionStateBase>
    {
        public void Set(HttpSessionStateBase repository, string name, object value)
        {
            repository[name] = value;
        }

        public void Remove(HttpSessionStateBase repository, string name)
        {
            repository.Remove(name);
        }

        public object Get(HttpSessionStateBase repository, string name)
        {
            return repository[name];
        }
    }
}