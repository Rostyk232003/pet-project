using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;
using LiteWebApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiteWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IWebHostEnvironment _env;

        // Конструктор тепер приймає всі три залежності
        public AccountController(
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            PasswordHasher<User> passwordHasher,
            IWebHostEnvironment env)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _passwordHasher = passwordHasher;
            _env = env;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var users = await _userRepository.GetAllAsync();
                if (users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Цей Email вже зайнятий");
                    return View(model);
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    Role = "User"
                };

                newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

                await _userRepository.AddAsync(newUser);
                return RedirectToAction("Login");
            }
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user == null) return NotFound();

            // Завантажуємо дані для профілю разом з історією замовлень
            var model = new ProfileViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Orders = await _orderRepository.GetOrdersByEmailAsync(email)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var email = User.Identity?.Name;
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.BirthDate = model.BirthDate;

                if (model.ProfileImage != null)
                {
                    // 1. Перевірка папки
                    string folderPath = Path.Combine(_env.WebRootPath, "images", "profiles");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    // 2. Формуємо ім'я: GUID + "_" + Оригінальна назва
                    // Використовуємо Path.GetFileName, щоб обрізати шляхи, які можуть передавати деякі браузери
                    string originalName = Path.GetFileName(model.ProfileImage.FileName);
                    string fileName = $"{Guid.NewGuid()}_{originalName}";

                    string filePath = Path.Combine(folderPath, fileName);

                    // 3. Збереження файлу
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(stream);
                    }

                    // 4. Оновлення шляху в базі
                    user.ProfilePictureUrl = "/images/profiles/" + fileName;
                }

                await _userRepository.UpdateAsync(user);
                TempData["SuccessMessage"] = "Профіль успішно оновлено!";
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetByEmailAsync(model.Email);
                if (user != null)
                {
                    // Перевірка пароля через PasswordHasher
                    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                    if (result == PasswordVerificationResult.Success)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Email),
                            new Claim(ClaimTypes.Role, user.Role)
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                        if (user.Role == "Admin")
                            return RedirectToAction("Index", "Admin");

                        return RedirectToAction("Index", "Product");
                    }
                }
                ModelState.AddModelError("", "Невірний Email або пароль");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Product");
        }
    }
}
