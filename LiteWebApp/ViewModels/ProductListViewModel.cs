namespace LiteWebApp.ViewModels
{
    public class ProductListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty; // Назва замість ID
        public string ImageUrl { get; set; } = string.Empty;
    }
}
