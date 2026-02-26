using LiteWebApp.Core.Entities;

namespace LiteWebApp.Core.Interfaces
{
    public interface IProductRepository
    {
        // Отримати всі товари (асинхронно)
        Task<IEnumerable<Product>> GetAllAsync();

        // Знайти один товар за його унікальним ID
        Task<Product?> GetByIdAsync(Guid id);

        // Додати новий товар
        Task AddAsync(Product product);

        // Оновити дані існуючого товару
        Task UpdateAsync(Product product);

        // Видалити товар за ID
        Task DeleteAsync(Guid id);
    }
}