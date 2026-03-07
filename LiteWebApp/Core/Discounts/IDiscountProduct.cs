// PROMPT v3.0: Інтерфейс для продукту знижки
namespace LiteWebApp.Core.Discounts
{
  public interface IDiscountProduct
  {
    decimal Apply(decimal price);
  }
}
