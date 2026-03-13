using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;
using LiteWebApp.Infrastructure.Data;
using LiteWebApp.Infrastructure.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ������� CHAIN OF RESPONSIBILITY ---
builder.Services.AddScoped<IHandler>(sp =>
{
    var authHandler = new AuthenticationCheckHandler();
    var orderHandler = new OrderPermissionHandler();
    var adminHandler = new AdminCRUDHandler();

    // �'������ �����: Auth -> Order -> Admin
    authHandler.SetNext(orderHandler).SetNext(adminHandler);

    return authHandler;
});

// --- 2. ��������в� �� ���²�� ����� ---
// ����������� �� Singleton (���� ��������� � ��� �� SemaphoreSlim)
builder.Services.AddSingleton<IUserRepository, JsonUserRepository>();
builder.Services.AddScoped<ICategoryRepository, JsonCategoryRepository>();
// PROMPT v4.0: DI Proxy для IProductRepository
builder.Services.AddScoped<IProductRepository>(sp =>
{
    var realRepo = sp.GetRequiredService<JsonProductRepository>();
    return new ProductRepositoryProxy(realRepo);
});
builder.Services.AddScoped<JsonProductRepository>();
builder.Services.AddScoped<IOrderRepository, JsonOrderRepository>();

// Реєстрація OrderHistoryRepository для IOrderHistoryRepository
builder.Services.AddScoped<IOrderHistoryRepository>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var filePath = Path.Combine(env.ContentRootPath, "Infrastructure", "Storage", "order_status_history.json");
    return new OrderHistoryRepository(filePath);
});

builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddHttpContextAccessor();

// --- 3. ��������ֲ� �� ��Ѳ� ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- 4. ����Ū� ������� ����Ҳ� (MIDDLEWARE) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // ������ �� ������

app.UseAuthentication(); // ��� ��?
app.UseAuthorization();  // �� ��� �����?

app.MapControllerRoute(
    name: "report",
    pattern: "Report/{action=Index}/{id?}",
    defaults: new { controller = "Report" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
