using System.Data.Entity.Migrations;
using System.Linq;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.EfContexts;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Realizations
{
    public class EfProductRepository : IDbProductRepository
    {
        public Product[] GetAll()
        {
            using (var context = new EfProductContext())
            {
                return context.ProductTable.OrderBy(p => p.Id).ToArray();
            }
        }

        public bool AddOrUpdate(Product product)
        {
            using (var context = new EfProductContext())
            {
                context.ProductTable.AddOrUpdate(product);
                return context.SaveChanges() > 0;
            }
            
        }

        public bool RemoveById(int id)
        {
            using (var context = new EfProductContext())
            {
                var entity = context.ProductTable.Find(id);
                if (entity == null)
                    return false;

                context.ProductTable.Remove(entity);
                context.SaveChanges();
                return true;
            }
            
        }

        public Product GetById(int id)
        {
            using (var context = new EfProductContext())
            {
                return context.ProductTable.Find(id);
            }
        }

        public Product[] SearchByName(Product[] products, string searchName)
        {
            return products.Where(p => p.Name.ToLower().Contains(searchName.ToLower())).ToArray();
        }

        public Product[] SearchByCategory(Product[] products, string searchCategory)
        {
            return products.Where(p => p.Category.ToLower().Contains(searchCategory.ToLower())).Select(p => p).ToArray();
        }
    }
}