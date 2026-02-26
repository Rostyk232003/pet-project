using System.Text.Json;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace LiteWebApp.Infrastructure.Data
{
    public class JsonUserRepository : IUserRepository
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public JsonUserRepository(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "Infrastructure", "Storage", "users.json");

            // Створюємо папку, якщо її не існує
            var directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            if (!File.Exists(_filePath)) return new List<User>();

            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<User>();

            return JsonSerializer.Deserialize<IEnumerable<User>>(json) ?? new List<User>();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var users = await GetAllAsync();
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task AddAsync(User user)
        {
            await _semaphore.WaitAsync();
            try
            {
                var users = (await GetAllAsync()).ToList();
                users.Add(user);
                await SaveAllAsync(users); // Використовуємо спільний метод
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateAsync(User user)
        {
            await _semaphore.WaitAsync(); // Додаємо захист для оновлення
            try
            {
                var users = (await GetAllAsync()).ToList();
                var index = users.FindIndex(u => u.Id == user.Id);
                if (index != -1)
                {
                    users[index] = user;
                    await SaveAllAsync(users); // Викликаємо метод, який раніше був відсутній
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // --- ДОДАНИЙ ПРИВАТНИЙ МЕТОД ---
        private async Task SaveAllAsync(List<User> users)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(users, options);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
