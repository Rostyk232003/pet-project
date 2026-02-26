namespace LiteWebApp.ViewModels
{
    public class ProfileViewModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public IFormFile? ProfileImage { get; set; }

        // Список замовлень для історії
        public List<LiteWebApp.Core.Entities.Order> Orders { get; set; } = new();
    }
}
