using System.Linq;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Contracts
{
    public interface IDbProductRepository
    {
        IQueryable<Product> GetAll();

        bool AddOrUpdate(Product product);

        bool RemoveById(int id);

        Product GetById(int id);

        IQueryable<Product> SearchByName(IQueryable<Product> products, string searchName);

        IQueryable<Product> SearchByCategory(IQueryable<Product> products, string searchCategory);
    }
}