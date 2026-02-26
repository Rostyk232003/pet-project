using LiteWebApp.ViewModels;

namespace LiteWebApp.Core.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerEmail { get; set; } = string.Empty;

        // Дані клієнта
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Comment { get; set; }

        // Товари та гроші
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Нове"; // Статус для адміна
        // PROMPT v1.0: Memento
        public OrderMemento CreateMemento(string adminEmail, string comment)
        {
            return new OrderMemento(Status, adminEmail, comment);
        }

        public void Restore(OrderMemento memento)
        {
            Status = memento.Status;
            // Можна додати логіку для коментаря, ChangedBy, ChangedAt якщо потрібно
        }
    }
}
