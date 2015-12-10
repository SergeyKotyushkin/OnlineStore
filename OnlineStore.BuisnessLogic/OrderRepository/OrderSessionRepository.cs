using System.Collections.Generic;
using System.Web;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.OrderRepository.Contracts;
using OnlineStore.BuisnessLogic.StorageRepository.Contracts;

namespace OnlineStore.BuisnessLogic.OrderRepository
{
    public class OrderSessionRepository : IOrderRepository<HttpSessionStateBase>
    {
        private readonly IStorageRepository<HttpSessionStateBase> _storageRepository;

        public OrderSessionRepository(IStorageRepository<HttpSessionStateBase> storageRepository)
        {
            _storageRepository = storageRepository;
        }

        public int Add(HttpSessionStateBase repository, string name, int id)
        {
            var orders = GetAll(repository, name);

            var order = orders.Find(o => o.Id == id);
            if (order == null)
            {
                order = new Order {Id = id};
                orders.Add(order);
            }
            order.Count++;

            SetAll(repository, orders, name);

            return order.Count;
        }

        public int Remove(HttpSessionStateBase repository, string name, int id)
        {
            var orders = GetAll(repository, name);

            var order = orders.Find(o => o.Id == id);
            if (order == null || order.Count == 0) return 0;
            if (order.Count == 1) orders.Remove(order);

            order.Count--;
            SetAll(repository, orders, name);

            return order.Count;
        }

        public List<Order> GetAll(HttpSessionStateBase repository, string name)
        {
            return _storageRepository.Get(repository, name) as List<Order> ?? new List<Order>();
        }

        private void SetAll(HttpSessionStateBase repository, IEnumerable<Order> orders, string name)
        {
            _storageRepository.Set(repository, name, orders);
        }
    }
}