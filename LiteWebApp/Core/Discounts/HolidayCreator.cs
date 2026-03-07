// PROMPT v3.0: Конкретний творець — святкова знижка
namespace LiteWebApp.Core.Discounts
{
  public class HolidayCreator : DiscountCreator
  {
    private readonly decimal _percent;
    public HolidayCreator(decimal percent)
    {
      _percent = percent;
    }
    public override IDiscountProduct CreateDiscount()
    {
      return new HolidayDiscount(_percent);
    }
  }
}
