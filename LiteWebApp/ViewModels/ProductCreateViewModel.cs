using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LiteWebApp.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Введіть назву товару")]
        [Display(Name = "Назва продукту")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Опис")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть ціну")]
        [Range(0.01, 1000000, ErrorMessage = "Ціна повинна бути додатною")]
        [Display(Name = "Ціна (грн)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        [Display(Name = "Категорія")]
        public Guid CategoryId { get; set; }

        [Display(Name = "Характеристики")]
        public List<CharacteristicItemViewModel> Characteristics { get; set; } = new();

        // Для випадаючого списку категорій
        public List<SelectListItem>? Categories { get; set; }

        // Для завантаження файлу
        [Display(Name = "Фото продукту")]
        public IFormFile? ImageFile { get; set; }
    }
}