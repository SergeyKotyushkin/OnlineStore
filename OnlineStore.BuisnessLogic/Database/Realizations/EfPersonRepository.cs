using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.EfContexts;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Realizations
{
    public class EfPersonRepository : IDbPersonRepository
    {
        public List<Person> GetAll()
        {
            using (var context = new EfPersonContext())
            {
                return context.PersonTable.ToList();
            }
        }

        public Person GetByName(string name)
        {
            using (var context = new EfPersonContext())
            {
                return context.PersonTable.Find(name);
            }
        }

        public bool AddOrUpdate(Person person)
        {
            using (var context = new EfPersonContext())
            {
                context.PersonTable.AddOrUpdate(person);
                return context.SaveChanges() > 0;
            }
        }
    }
}
