// PROMPT v3.0: Продукт знижки — перше замовлення (фіксована сума)
namespace LiteWebApp.Core.Discounts
{
  public class FirstOrderDiscount : IDiscountProduct
  {
    private readonly decimal _discountAmount;
    public FirstOrderDiscount(decimal discountAmount)
    {
      _discountAmount = discountAmount;
    }
    public decimal Apply(decimal price)
    {
      return price - _discountAmount > 0 ? price - _discountAmount : 0;
    }
  }
}
