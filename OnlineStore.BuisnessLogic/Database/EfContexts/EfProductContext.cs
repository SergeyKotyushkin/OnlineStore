using System.Data.Entity;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.EfContexts
{
    public class EfProductContext : DbContext
    {
        public EfProductContext()
            : base("name=EfProductContext")
        {
        }

        public virtual DbSet<Product> ProductTable { get; set; }
    }
}
