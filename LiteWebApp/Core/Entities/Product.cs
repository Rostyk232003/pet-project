namespace LiteWebApp.Core.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Зв'язок із категорією
        public Guid CategoryId { get; set; }

        public string ImageUrl { get; set; } = "/images/products/default.png";
        public Dictionary<string, string> Characteristics { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
