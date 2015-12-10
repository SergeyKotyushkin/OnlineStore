using System.Data.Entity.Migrations;
using System.Linq;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.EfContexts;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Realizations
{
    public class EfProductRepository : IDbProductRepository
    {
        private readonly EfProductContext _context;

        public EfProductRepository(EfProductContext context)
        {
            _context = context;
        }

        public IQueryable<Product> GetAll()
        {
            return _context.ProductTable.OrderBy(p => p.Id);
        }

        public bool AddOrUpdate(Product product)
        {
            _context.ProductTable.AddOrUpdate(product);
            return _context.SaveChanges() > 0;
        }

        public bool RemoveById(int id)
        {
            var entity = _context.ProductTable.Find(id);
            if (entity == null) return false;
            _context.ProductTable.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public Product GetById(int id)
        {
            return _context.ProductTable.Find(id);
        }

        public IQueryable<Product> SearchByName(IQueryable<Product> products, string searchName)
        {
            return products.Where(p => p.Name.ToLower().Contains(searchName.ToLower())).Select(p => p);
        }

        public IQueryable<Product> SearchByCategory(IQueryable<Product> products, string searchCategory)
        {
            return products.Where(p => p.Category.ToLower().Contains(searchCategory.ToLower())).Select(p => p);
        }
    }
}