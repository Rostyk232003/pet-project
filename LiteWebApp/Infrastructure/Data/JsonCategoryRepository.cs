using System.Text.Json;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;

namespace LiteWebApp.Infrastructure.Data
{
    public class JsonCategoryRepository : ICategoryRepository
    {
        private readonly string _filePath;
        private List<Category> _cache = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public JsonCategoryRepository(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Infrastructure", "Storage", "categories.json");
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (!File.Exists(_filePath)) return;
                var json = File.ReadAllText(_filePath);
                _cache = JsonSerializer.Deserialize<List<Category>>(json) ?? new List<Category>();
            }
            catch { _cache = new List<Category>(); }
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

        public async Task<IEnumerable<Category>> GetAllAsync() => await Task.FromResult(_cache);
        public async Task<Category?> GetByIdAsync(Guid id) => await Task.FromResult(_cache.FirstOrDefault(c => c.Id == id));
        public async Task AddAsync(Category category) { _cache.Add(category); await SaveToFileAsync(); }
        public async Task UpdateAsync(Category category)
        {
            var index = _cache.FindIndex(c => c.Id == category.Id);
            if (index != -1) { _cache[index] = category; await SaveToFileAsync(); }
        }
        public async Task DeleteAsync(Guid id)
        {
            var category = _cache.FirstOrDefault(c => c.Id == id);
            if (category != null) { _cache.Remove(category); await SaveToFileAsync(); }
        }
    }
}
