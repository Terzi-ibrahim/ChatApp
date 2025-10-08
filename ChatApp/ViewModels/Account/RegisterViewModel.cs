using System.ComponentModel.DataAnnotations;

namespace ChatApp.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Adınız Zorunlu")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Soyadınız Zorunludur")]
        public string? Surname { get; set; }
        [Required(ErrorMessage = "Kullanıcı Adı Zorunludur")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Emalil Zorunludur")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Telefon Numarası Zorunludur")]
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = false;

        [Required(ErrorMessage = "Şifre Zorunludur")]
        public string? Password { get; set; }
    }
}
