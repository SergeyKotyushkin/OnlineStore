using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Contracts
{
    public interface IDbProductRepository
    {
        Product[] GetAll();

        Product[] GetByIds(params int[] ids);

        Product[] GetRange(int @from, int size);

        int GetCount();

        Product AddOrUpdate(Product product);

        Product RemoveById(int id);

        Product GetById(int id);

        Product[] SearchByName(Product[] products, string searchName);

        Product[] SearchByCategory(Product[] products, string searchCategory);
    }
}