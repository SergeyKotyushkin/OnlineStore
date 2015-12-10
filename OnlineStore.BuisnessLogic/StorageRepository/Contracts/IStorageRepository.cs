namespace OnlineStore.BuisnessLogic.StorageRepository.Contracts
{
    public interface IStorageRepository<in TRepository>
    {
        void Set(TRepository repository, string name, object value);

        void Remove(TRepository repository, string name);

        object Get(TRepository repository, string name);
    }
}