using System.Text.Json;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;

namespace LiteWebApp.Infrastructure.Data
{
    public class JsonOrderRepository : IOrderRepository
    {
        private readonly string _filePath;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public JsonOrderRepository(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Infrastructure", "Storage", "orders.json");
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            if (!File.Exists(_filePath)) return new List<Order>();
            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                if (string.IsNullOrWhiteSpace(json)) return new List<Order>();
                var orders = JsonSerializer.Deserialize<List<Order>>(json) ?? new List<Order>();
                return orders.OrderByDescending(o => o.OrderDate).ToList();
            }
            catch
            {
                return new List<Order>();
            }
        }

        public async Task SaveOrderAsync(Order order)
        {
            await _semaphore.WaitAsync();
            try
            {
                var orders = await GetAllOrdersAsync();
                if (order.Id == Guid.Empty) order.Id = Guid.NewGuid();
                orders.Add(order);
                await SaveToFileInternalAsync(orders);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            await _semaphore.WaitAsync();
            try
            {
                var orders = await GetAllOrdersAsync();
                var order = orders.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    order.Status = newStatus;
                    await SaveToFileInternalAsync(orders);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Order>> GetOrdersByEmailAsync(string email)
        {
            var allOrders = await GetAllOrdersAsync();
            return allOrders
                .Where(o => !string.IsNullOrEmpty(o.CustomerEmail) &&
                            o.CustomerEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            var orders = await GetAllOrdersAsync();
            return orders.FirstOrDefault(o => o.Id == id);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await _semaphore.WaitAsync();
            try
            {
                var orders = await GetAllOrdersAsync();
                var idx = orders.FindIndex(o => o.Id == order.Id);
                if (idx >= 0)
                {
                    orders[idx] = order;
                    await SaveToFileInternalAsync(orders);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SaveToFileInternalAsync(List<Order> orders)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(orders, options);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
