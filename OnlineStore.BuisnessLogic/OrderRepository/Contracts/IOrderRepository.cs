using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.OrderRepository.Contracts
{
    public interface IOrderRepository<in TRepository>
    {
        int Add(TRepository repository, string name, int id);

        int Remove(TRepository repository, string name, int id);

        List<Order> GetAll(TRepository repository, string name);

        Order[] GetRange(TRepository repository, string name, int @from, int size);

        int GetCount(TRepository repository, string name);
    }
}