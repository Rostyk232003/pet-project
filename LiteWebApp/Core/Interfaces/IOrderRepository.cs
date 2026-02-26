using LiteWebApp.Core.Entities;

namespace LiteWebApp.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task SaveOrderAsync(Order order);
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<Order>> GetOrdersByEmailAsync(string email);
        Task UpdateOrderStatusAsync(Guid orderId, string newStatus);

        Task<Order?> GetOrderByIdAsync(Guid id);
        Task UpdateOrderAsync(Order order);
    }
}
