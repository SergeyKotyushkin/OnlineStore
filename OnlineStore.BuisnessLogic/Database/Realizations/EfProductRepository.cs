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
                return context.ProductTable.ToArray();
            }
        }

        public Product[] GetByIds(params int[] ids)
        {
            using (var context = new EfProductContext())
            {
                return context.ProductTable.Where(p => ids.Contains(p.Id)).OrderBy(p => p.Id).ToArray();
            }
        }

        public Product[] GetRange(int from, int size)
        {
            using (var context = new EfProductContext())
            {
                var products = context.ProductTable.ToList();
                var count = products.Count;
                var rest = count - from*size;
                return products.GetRange(from*size, size > rest ? rest : size).ToArray();
            }
        }

        public int GetCount()
        {
            using (var context = new EfProductContext())
            {
                return context.ProductTable.Count();
            }
        }

        public Product AddOrUpdate(Product product)
        {
            using (var context = new EfProductContext())
            {
                context.ProductTable.AddOrUpdate(product);
                return context.SaveChanges() > 0 ? product : null;
            }
            
        }

        public Product RemoveById(int id)
        {
            using (var context = new EfProductContext())
            {
                var product = context.ProductTable.Find(id);
                if (product == null)
                    return null;

                context.ProductTable.Remove(product);
                return context.SaveChanges() > 0 ? product : null;
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