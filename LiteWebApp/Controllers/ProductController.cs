using Microsoft.AspNetCore.Mvc;
using LiteWebApp.Core.Interfaces;
using LiteWebApp.ViewModels;

namespace LiteWebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        // Впровадження залежностей (Dependency Injection)
        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Головна сторінка каталогу
        public async Task<IActionResult> Index()
        {
            // Отримуємо дані з обох репозиторіїв паралельно
            var productsTask = _productRepository.GetAllAsync();
            var categoriesTask = _categoryRepository.GetAllAsync();

            await Task.WhenAll(productsTask, categoriesTask);

            var products = await productsTask;
            var categories = await categoriesTask;

            // Логіка "Join" (з'єднання): перетворюємо Entity у ViewModel
            var viewModel = products.Select(p => new ProductListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                // Шукаємо назву категорії за її Id
                CategoryName = categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "Без категорії"
            });

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }
    }
}
