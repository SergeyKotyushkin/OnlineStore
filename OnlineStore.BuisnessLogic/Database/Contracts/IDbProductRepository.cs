using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Contracts
{
    public interface IDbProductRepository
    {
        Product[] GetAll();

        bool AddOrUpdate(Product product);

        bool RemoveById(int id);

        Product GetById(int id);

        Product[] SearchByName(Product[] products, string searchName);

        Product[] SearchByCategory(Product[] products, string searchCategory);
    }
}