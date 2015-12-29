using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.ElasticRepository.Contracts
{
    public interface IElasticRepository
    {
        void CheckConnection();

        void AddOrUpdate(ProductElasticDto product);

        void RemoveById(int id);

        int[] SearchByNameAndCategory(string name, string category, int from, int size);

        long GetCount(string name = null, string category = null);
    }
}