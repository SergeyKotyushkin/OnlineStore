using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Database.Contracts
{
    public interface IDbPersonRepository
    {
        List<Person> GetAll();

        Person GetByName(string name);

        bool AddOrUpdate(Person person);
    }
}