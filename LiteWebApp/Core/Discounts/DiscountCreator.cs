// PROMPT v3.0: Абстрактний творець знижки (Factory Method)
using LiteWebApp.Core.Discounts;

namespace LiteWebApp.Core.Discounts
{
  public abstract class DiscountCreator
  {
    // Фабричний метод
    public abstract IDiscountProduct CreateDiscount();

    // Базова логіка розрахунку фінальної ціни
    public decimal CalculateFinalPrice(decimal amount)
    {
      IDiscountProduct discount = CreateDiscount(); // PROMPT v3.1.2: Refactor var to IDiscountProduct
      return discount.Apply(amount);
    }
  }
}
