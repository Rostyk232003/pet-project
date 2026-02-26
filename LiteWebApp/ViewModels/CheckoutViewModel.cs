using System.ComponentModel.DataAnnotations;

namespace LiteWebApp.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Введіть прізвище та ім'я")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть номер телефону")]
        [Phone(ErrorMessage = "Некоректний формат телефону")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть адресу або відділення пошти")]
        public string DeliveryAddress { get; set; } = string.Empty;

        public string? Comment { get; set; }

        // Список товарів для перегляду під час оформлення
        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal GrandTotal => CartItems.Sum(x => x.Total);
    }
}
