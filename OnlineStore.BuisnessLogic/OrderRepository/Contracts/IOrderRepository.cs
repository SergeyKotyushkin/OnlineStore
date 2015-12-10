using System.Collections.Generic;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.OrderRepository.Contracts
{
    public interface IOrderRepository<in T>
    {
        int Add(T repository, string name, int id);

        int Remove(T repository, string name, int id);

        List<Order> GetAll(T repository, string name);
    }
}