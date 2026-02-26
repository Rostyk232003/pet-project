using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;
using LiteWebApp.Infrastructure.Handlers;
using LiteWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LiteWebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHandler _permissionChain;
        private const string CartSessionKey = "UserCart";

        public CartController(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IHandler permissionChain)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _permissionChain = permissionChain;
        }

        // --- ВІДОБРАЖЕННЯ КОШИКА ---

        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // --- ДОДАВАННЯ ТА КЕРУВАННЯ ТОВАРАМИ ---

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return NotFound();

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item == null)
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                });
            }
            else
            {
                item.Quantity++;
            }

            SaveCartToSession(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(Guid productId, int change)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                item.Quantity += change;
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
            }

            SaveCartToSession(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(Guid productId)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction("Index");
        }

        // --- ОФОРМЛЕННЯ ЗАМОВЛЕННЯ (CHECKOUT) З ВИКОРИСТАННЯМ ПАТТЕРНУ ---

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            // 1. Отримуємо поточного користувача за його Email
            var email = User.Identity?.Name;
            var user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Email == email);

            // 2. Перевірка через ланцюжок (Дія: CHECKOUT)
            // Якщо користувач гість — ланцюжок поверне false
            if (!_permissionChain.Handle(user, "CHECKOUT"))
            {
                if (user == null) return RedirectToAction("Login", "Account");
                return RedirectToAction("AccessDenied", "Account");
            }

            var cart = GetCartFromSession();
            if (!cart.Any()) return RedirectToAction("Index");

            var model = new CheckoutViewModel
            {
                CartItems = cart,
                // Передзаповнення імені, якщо користувач авторизований
                FullName = user != null ? $"{user.FirstName} {user.LastName}" : ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var email = User.Identity?.Name;
            var user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Email == email);

            // Повторна перевірка безпеки ланцюжком
            if (!_permissionChain.Handle(user, "CHECKOUT"))
            {
                if (user == null) return RedirectToAction("Login", "Account");
                return RedirectToAction("AccessDenied", "Account");
            }

            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                ModelState.AddModelError("", "Ваш кошик порожній");
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    CustomerEmail = user?.Email ?? string.Empty,
                    CustomerName = model.FullName,
                    Phone = model.Phone,
                    Address = model.DeliveryAddress,
                    Comment = model.Comment,
                    Items = cart,
                    TotalAmount = cart.Sum(x => x.Total),
                    OrderDate = DateTime.Now,
                    Status = "Нове"
                };

                await _orderRepository.SaveOrderAsync(order);

                // Очищаємо кошик після успішного замовлення
                HttpContext.Session.Remove(CartSessionKey);
                return RedirectToAction("OrderConfirmation");
            }

            model.CartItems = cart;
            return View(model);
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        // --- ДОПОМІЖНІ МЕТОДИ ДЛЯ СЕСІЇ ---

        private List<CartItemViewModel> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            return json == null
                ? new List<CartItemViewModel>()
                : JsonSerializer.Deserialize<List<CartItemViewModel>>(json) ?? new List<CartItemViewModel>();
        }

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }
}
