using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LiteWebApp.ViewModels
{
    public class ProductEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Назва обов'язкова")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Опис обов'язковий")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Ціна повинна бути більше 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        public Guid CategoryId { get; set; }

        public string? ExistingImageUrl { get; set; } // Шлях до поточного фото
        public IFormFile? NewImageFile { get; set; }  // Для нового фото (необов'язково)

        [Display(Name = "Характеристики")]
        public List<CharacteristicItemViewModel> Characteristics { get; set; } = new();

        public List<SelectListItem>? Categories { get; set; }
    }
}