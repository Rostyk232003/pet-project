using System.ComponentModel.DataAnnotations;

namespace LiteWebApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
