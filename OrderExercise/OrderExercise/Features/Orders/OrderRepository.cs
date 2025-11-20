using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Features.Orders
{
    public class OrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new();

        public Task AddAsync(Order order)
        {
            order.Id = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task<Order?> GetByISBNAsync(string isbn)
        {
            var order = _orders.FirstOrDefault(o => 
                string.Equals(o.ISBN, isbn, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(order);
        }

        public Task<bool> ExistsTitleForAuthorAsync(string title, string author)
        {
            var exists = _orders.Any(o =>
                string.Equals(o.Title, title, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(o.Author, author, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(exists);
        }

        public Task<bool> ExistsISBNAsync(string isbn)
        {
            var exists = _orders.Any(o => 
                string.Equals(o.ISBN, isbn, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(exists);
        }

        public Task<int> CountOrdersAddedTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            var count = _orders.Count(o => o.CreatedAt.Date == today);
            return Task.FromResult(count);
        }
    }
}