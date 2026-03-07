// PROMPT v3.0: Конкретний творець — без знижки
namespace LiteWebApp.Core.Discounts
{
  public class DefaultCreator : DiscountCreator
  {
    public override IDiscountProduct CreateDiscount()
    {
      return new DefaultNoDiscount();
    }
  }
}
