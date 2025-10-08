using ChatApp.Models;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.ViewModels.Account
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "Kullanıcı adı, e-posta veya telefon zorunludur")]
        public string? Identifier { get; set; }
        [Required(ErrorMessage = "Şifre zorunludur")]
        public string? Password { get; set; }       

    }
}
