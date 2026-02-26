using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;
using LiteWebApp.Infrastructure.Handlers;
using LiteWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LiteWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHandler _permissionChain;
        private readonly IOrderHistoryRepository _historyRepository;

        public AdminController(IProductRepository productRepository,
                               ICategoryRepository categoryRepository,
                               IWebHostEnvironment env,
                               IOrderRepository orderRepository,
                               IUserRepository userRepository,
                               IHandler permissionChain,
                               IOrderHistoryRepository historyRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _env = env;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _permissionChain = permissionChain;
            _historyRepository = historyRepository;
        }

        // --- ДОПОМІЖНИЙ МЕТОД ПЕРЕВІРКИ (ПАТТЕРН CoR) ---
        private async Task<bool> CheckAccessAsync(string action)
        {
            var email = User.Identity?.Name;
            var user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Email == email);
            return _permissionChain.Handle(user, action);
        }

        // --- ЛОГІКА ЗАМОВЛЕНЬ ---

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            if (!await CheckAccessAsync("PRODUCT_EDIT")) // Використовуємо права адміна
                return RedirectToAction("AccessDenied", "Account");

            var orders = await _orderRepository.GetAllOrdersAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid orderId, string newStatus)
        {
            if (!await CheckAccessAsync("PRODUCT_EDIT"))
                return RedirectToAction("AccessDenied", "Account");

            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Замовлення не знайдено.";
                    return RedirectToAction(nameof(Orders));
                }
                // Створюємо знімок перед зміною
                var memento = order.CreateMemento(User.Identity?.Name ?? "admin", "Зміна статусу на " + newStatus);
                await _historyRepository.SaveSnapshotAsync(orderId, memento);
                order.Status = newStatus;
                await _orderRepository.UpdateOrderAsync(order);
                TempData["SuccessMessage"] = $"Статус замовлення успішно змінено на \"{newStatus}\"";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Сталася помилка при оновленні статусу.";
            }
            return RedirectToAction(nameof(Orders));
        }

        // PROMPT v1.0: UndoStatusChange
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UndoStatusChange(Guid orderId)
        {
            if (!await CheckAccessAsync("PRODUCT_EDIT"))
                return RedirectToAction("AccessDenied", "Account");

            var history = (await _historyRepository.GetHistoryAsync(orderId)).ToList();
            if (!history.Any())
            {
                TempData["ErrorMessage"] = "Історія змін порожня.";
                return RedirectToAction(nameof(Orders));
            }
            var lastMemento = history.Last();
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Замовлення не знайдено.";
                return RedirectToAction(nameof(Orders));
            }
            order.Restore(lastMemento);
            await _orderRepository.UpdateOrderAsync(order);
            TempData["SuccessMessage"] = $"Статус замовлення відновлено до \"{lastMemento.Status}\".";
            return RedirectToAction(nameof(Orders));
            // ...existing code...
        }

        // --- ЛОГІКА КАТЕГОРІЙ ---

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            if (!await CheckAccessAsync("PRODUCT_EDIT"))
                return RedirectToAction("AccessDenied", "Account");

            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(Guid? id, string name)
        {
            // 1. Динамічне визначення необхідного права доступу
            // Якщо id порожній або дорівнює Guid.Empty — це створення нової категорії.
            string requiredAction = (id == null || id == Guid.Empty) ? "PRODUCT_CREATE" : "PRODUCT_EDIT";

            // 2. Перевірка через ланцюжок обов'язків (CoR)
            if (!await CheckAccessAsync(requiredAction))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // 3. Валідація вхідних даних
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Назва категорії не може бути порожньою.";
                return RedirectToAction(nameof(Categories));
            }

            // 4. Перевірка на унікальність назви категорії
            var allCategories = await _categoryRepository.GetAllAsync();
            if (allCategories.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) && c.Id != id))
            {
                TempData["ErrorMessage"] = $"Категорія з назвою «{name}» вже існує!";
                return RedirectToAction(nameof(Categories));
            }

            // 5. Виконання операції (Додавання або Оновлення)
            if (id == null || id == Guid.Empty)
            {
                // Створення нової категорії
                await _categoryRepository.AddAsync(new Category
                {
                    Id = Guid.NewGuid(),
                    Name = name.Trim()
                });
                TempData["SuccessMessage"] = "Нову категорію успішно додано.";
            }
            else
            {
                // Редагування існуючої категорії
                var existing = await _categoryRepository.GetByIdAsync(id.Value);
                if (existing != null)
                {
                    existing.Name = name.Trim();
                    await _categoryRepository.UpdateAsync(existing);
                    TempData["SuccessMessage"] = "Категорію оновлено.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Категорію не знайдено.";
                }
            }

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            if (!await CheckAccessAsync("PRODUCT_DELETE"))
                return RedirectToAction("AccessDenied", "Account");

            await _categoryRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Категорію видалено.";
            return RedirectToAction(nameof(Categories));
        }

        // --- ЛОГІКА ТОВАРІВ ---

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await CheckAccessAsync("PRODUCT_EDIT"))
                return RedirectToAction("AccessDenied", "Account");

            var products = await _productRepository.GetAllAsync();
            return View(products.OrderByDescending(p => p.CreatedAt));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!await CheckAccessAsync("PRODUCT_CREATE"))
                return RedirectToAction("AccessDenied", "Account");

            var categories = await _categoryRepository.GetAllAsync();
            var model = new ProductCreateViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList(),
                Characteristics = new List<CharacteristicItemViewModel>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!await CheckAccessAsync("PRODUCT_CREATE"))
                return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                string imagePath = "/images/products/default.png";

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string originalFileName = Path.GetFileName(model.ImageFile.FileName);
                    string uniqueFileName = $"{Guid.NewGuid().ToString().Substring(0, 6)}_{originalFileName}";
                    string uploadFolder = Path.Combine(_env.WebRootPath, "images", "products");
                    string fullPath = Path.Combine(uploadFolder, uniqueFileName);

                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                    imagePath = $"/images/products/{uniqueFileName}";
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    ImageUrl = imagePath,
                    CreatedAt = DateTime.UtcNow,
                    Characteristics = model.Characteristics?
                        .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                        .ToDictionary(x => x.Key.Trim(), x => x.Value?.Trim() ?? "")
                        ?? new Dictionary<string, string>()
                };

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var cats = await _categoryRepository.GetAllAsync();
            model.Categories = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!await CheckAccessAsync("PRODUCT_EDIT"))
                return RedirectToAction("AccessDenied", "Account");

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var categories = await _categoryRepository.GetAllAsync();

            var model = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,
                Characteristics = product.Characteristics?
                    .Select(x => new CharacteristicItemViewModel { Key = x.Key, Value = x.Value })
                    .ToList() ?? new List<CharacteristicItemViewModel>(),
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditViewModel model)
        {
            if (!await CheckAccessAsync("PRODUCT_EDIT"))
                return RedirectToAction("AccessDenied", "Account");

            if (ModelState.IsValid)
            {
                var product = await _productRepository.GetByIdAsync(model.Id);
                if (product == null) return NotFound();

                string imagePath = product.ImageUrl;

                if (model.NewImageFile != null && model.NewImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default.png"))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    }

                    string uniqueFileName = $"{Guid.NewGuid().ToString().Substring(0, 6)}_{model.NewImageFile.FileName}";
                    string uploadFolder = Path.Combine(_env.WebRootPath, "images", "products");
                    string fullPath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.NewImageFile.CopyToAsync(fileStream);
                    }
                    imagePath = $"/images/products/{uniqueFileName}";
                }

                product.Name = model.Name;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;
                product.Description = model.Description;
                product.ImageUrl = imagePath;

                product.Characteristics = model.Characteristics?
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                    .ToDictionary(x => x.Key.Trim(), x => x.Value?.Trim() ?? "")
                    ?? new Dictionary<string, string>();

                await _productRepository.UpdateAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var cats = await _categoryRepository.GetAllAsync();
            model.Categories = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await CheckAccessAsync("PRODUCT_DELETE"))
                return RedirectToAction("AccessDenied", "Account");

            var product = await _productRepository.GetByIdAsync(id);

            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default.png"))
                {
                    var filePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                await _productRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = $"Товар «{product.Name}» було остаточно видалено.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
