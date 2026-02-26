using System.ComponentModel.DataAnnotations;

namespace LiteWebApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [Display(Name = "Ім'я")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Прізвище обов'язкове")]
        [Display(Name = "Прізвище")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Дата народження")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Пароль має бути не менше 6 символів")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        [DataType(DataType.Password)]
        [Display(Name = "Підтвердіть пароль")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
