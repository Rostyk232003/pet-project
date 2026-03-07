// PROMPT v3.0: Продукт знижки — без знижки
namespace LiteWebApp.Core.Discounts
{
  public class DefaultNoDiscount : IDiscountProduct
  {
    public decimal Apply(decimal price)
    {
      return price;
    }
  }
}
