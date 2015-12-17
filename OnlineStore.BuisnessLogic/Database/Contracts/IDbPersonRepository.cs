using System.Linq;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Contracts
{
    public interface IDbPersonRepository
    {
        IQueryable<Person> GetAll();

        bool AddOrUpdate(Person person);
    }
}