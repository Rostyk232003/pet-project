// PROMPT v4.1.1: Створення файлу Constants для кешу
namespace LiteWebApp.Helpers
{
  public static class Constants
  {
    // PROMPT v4.1.1: Configurable TTL
    public const string CacheSettingsSection = "CacheSettings";
    public const string ProductCacheTTLKey = "ProductCacheTTL";
    public const int DefaultProductCacheTTLMinutes = 5;
  }
}
