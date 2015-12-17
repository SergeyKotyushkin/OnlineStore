using System.Data.Entity;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.EfContexts
{
    public class EfPersonContext : DbContext
    {
        public EfPersonContext()
            : base("name=EfPersonContext")
        {
        }

        public DbSet<Person> PersonTable { get; set; }
        public DbSet<OrderHistory> OrdersHistoryTable { get; set; }
    }
}
