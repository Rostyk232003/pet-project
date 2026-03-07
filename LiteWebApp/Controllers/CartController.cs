using LiteWebApp.Core.Discounts;
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
            // PROMPT v3.1.4: ToT-рефакторинг Factory Method (Checkout)
            string? email = User.Identity?.Name;
            User? user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Email == email);

            // 2. Перевірка через ланцюжок (Дія: CHECKOUT)
            // Якщо користувач гість — ланцюжок поверне false
            if (!_permissionChain.Handle(user, "CHECKOUT"))
            {
                if (user == null) return RedirectToAction("Login", "Account");
                return RedirectToAction("AccessDenied", "Account");
            }

            List<CartItemViewModel> cart = GetCartFromSession();
            if (!cart.Any()) return RedirectToAction("Index");

            // --- Вибір типу знижки ---
            decimal discountAmount = 0;
            string discountType = string.Empty;
            decimal finalTotal = cart.Sum(x => x.Total);

            int orderCount = user != null ? (await _orderRepository.GetAllOrdersAsync()).Count(o => o.CustomerEmail == user.Email) : 0;
            DateTime today = DateTime.Today;
            LiteWebApp.Core.Discounts.DiscountCreator discountCreator;
            if (user != null && user.BirthDate.HasValue && user.BirthDate.Value.Month == today.Month && user.BirthDate.Value.Day == today.Day)
            {
                // День народження — 10% знижка
                discountCreator = new Core.Discounts.HolidayCreator(10);
                discountType = "Знижка до дня народження (10%)";
            }
            else if (user != null && orderCount == 0)
            {
                // Перше замовлення — 100 грн знижка
                discountCreator = new Core.Discounts.FirstOrderCreator(100);
                discountType = "Знижка на перше замовлення (100 грн)";
            }
            else
            {
                discountCreator = new Core.Discounts.DefaultCreator();
                discountType = "Без знижки";
            }
            finalTotal = discountCreator.CalculateFinalPrice(cart.Sum(x => x.Total));
            discountAmount = cart.Sum(x => x.Total) - finalTotal;

            var model = new CheckoutViewModel
            {
                CartItems = cart,
                FullName = user != null ? $"{user.FirstName} {user.LastName}" : "",
                DiscountAmount = discountAmount,
                DiscountType = discountType,
                FinalTotal = finalTotal
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            // PROMPT v3.1.4: ToT-рефакторинг Factory Method (Checkout POST)
            string? email = User.Identity?.Name;
            User? user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Email == email);

            // Повторна перевірка безпеки ланцюжком
            if (!_permissionChain.Handle(user, "CHECKOUT"))
            {
                if (user == null) return RedirectToAction("Login", "Account");
                return RedirectToAction("AccessDenied", "Account");
            }

            List<CartItemViewModel> cart = GetCartFromSession();
            if (!cart.Any())
            {
                ModelState.AddModelError("", "Ваш кошик порожній");
                return RedirectToAction("Index");
            }

            // --- Повторний розрахунок знижки для коректного запису ---
            decimal discountAmount = 0;
            string discountType = string.Empty;
            decimal finalTotal = cart.Sum(x => x.Total);

            int orderCount = user != null ? (await _orderRepository.GetAllOrdersAsync()).Count(o => o.CustomerEmail == user.Email) : 0;
            DateTime today = DateTime.Today;
            LiteWebApp.Core.Discounts.DiscountCreator discountCreator;
            if (user != null && user.BirthDate.HasValue && user.BirthDate.Value.Month == today.Month && user.BirthDate.Value.Day == today.Day)
            {
                discountCreator = new Core.Discounts.HolidayCreator(10);
                discountType = "Знижка до дня народження (10%)";
            }
            else if (user != null && orderCount == 0)
            {
                discountCreator = new Core.Discounts.FirstOrderCreator(100);
                discountType = "Знижка на перше замовлення (100 грн)";
            }
            else
            {
                discountCreator = new Core.Discounts.DefaultCreator();
                discountType = "Без знижки";
            }
            finalTotal = discountCreator.CalculateFinalPrice(cart.Sum(x => x.Total));
            discountAmount = cart.Sum(x => x.Total) - finalTotal;

            if (ModelState.IsValid)
            {
                decimal total = cart.Sum(x => x.Total);
                decimal discounted = (discountAmount > 0 && finalTotal > 0 && finalTotal < total)
                    ? finalTotal
                    : total;

                var order = new Order
                {
                    CustomerEmail = user?.Email ?? string.Empty,
                    CustomerName = model.FullName,
                    Phone = model.Phone,
                    Address = model.DeliveryAddress,
                    Comment = model.Comment,
                    Items = cart,
                    TotalAmount = total,
                    DiscountedTotal = discounted,
                    OrderDate = DateTime.Now,
                    Status = "Нове"
                };

                await _orderRepository.SaveOrderAsync(order);

                // Очищаємо кошик після успішного замовлення
                HttpContext.Session.Remove(CartSessionKey);
                return RedirectToAction("OrderConfirmation");
            }

            model.CartItems = cart;
            // Для View — оновити поля знижки
            model.DiscountAmount = discountAmount;
            model.DiscountType = discountType;
            model.FinalTotal = finalTotal;
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
