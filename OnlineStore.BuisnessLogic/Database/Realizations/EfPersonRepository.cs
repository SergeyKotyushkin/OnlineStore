using System.Data.Entity.Migrations;
using System.Linq;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.EfContexts;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Realizations
{
    public class EfPersonRepository : IDbPersonRepository
    {
        private readonly EfPersonContext _context = new EfPersonContext();
        
        public IQueryable<Person> GetAll()
        {
            return _context.PersonTable;
        }
        
        public bool AddOrUpdate(Person person)
        {
            _context.PersonTable.AddOrUpdate(person);
            return _context.SaveChanges() > 0;
        }
    }
}
