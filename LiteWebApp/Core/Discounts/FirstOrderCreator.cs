// PROMPT v3.0: Конкретний творець — перше замовлення
namespace LiteWebApp.Core.Discounts
{
  public class FirstOrderCreator : DiscountCreator
  {
    private readonly decimal _discountAmount;
    public FirstOrderCreator(decimal discountAmount)
    {
      _discountAmount = discountAmount;
    }
    public override IDiscountProduct CreateDiscount()
    {
      return new FirstOrderDiscount(_discountAmount);
    }
  }
}
