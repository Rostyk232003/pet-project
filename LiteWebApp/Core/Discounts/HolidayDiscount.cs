// PROMPT v3.0: Продукт знижки — святкова (відсоток)
namespace LiteWebApp.Core.Discounts
{
  public class HolidayDiscount : IDiscountProduct
  {
    private readonly decimal _percent;
    public HolidayDiscount(decimal percent)
    {
      _percent = percent;
    }
    public decimal Apply(decimal price)
    {
      return price * (1 - _percent / 100);
    }
  }
}
