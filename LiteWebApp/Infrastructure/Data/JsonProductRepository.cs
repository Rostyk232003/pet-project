using System.Text.Json;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;

namespace LiteWebApp.Infrastructure.Data
{
    public class JsonProductRepository : IProductRepository
    {
        private readonly string _filePath;
        private List<Product> _cache = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public JsonProductRepository(IWebHostEnvironment env)
        {
            // Шлях до файлу продуктів
            _filePath = Path.Combine(env.ContentRootPath, "Infrastructure", "Storage", "products.json");
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (!File.Exists(_filePath)) return;
                var json = File.ReadAllText(_filePath);
                _cache = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
            }
            catch { _cache = new List<Product>(); }
        }

        private async Task SaveToFileAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_filePath, json);
            }
            finally { _semaphore.Release(); }
        }

        public async Task<IEnumerable<Product>> GetAllAsync() => await Task.FromResult(_cache);

        public async Task<Product?> GetByIdAsync(Guid id) => await Task.FromResult(_cache.FirstOrDefault(p => p.Id == id));

        public async Task AddAsync(Product product)
        {
            _cache.Add(product);
            await SaveToFileAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var index = _cache.FindIndex(p => p.Id == product.Id);
            if (index != -1)
            {
                _cache[index] = product;
                await SaveToFileAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = _cache.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _cache.Remove(product);
                await SaveToFileAsync();
            }
        }
    }
}
