using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class User
    {
        public int id { get; set; }

        [Required(ErrorMessage="Adınız Zorunlu")]
        public string? Name { get; set; }

        [Required(ErrorMessage ="Soyadınız Zorunludur")]
        public string? Surname { get; set; }
        [Required(ErrorMessage = "Kullanıcı Adı Zorunludur")]
        public string? UserName { get; set; }
        [Required(ErrorMessage ="Emalil Zorunludur")]
        public string? Email { get; set; }
        [Required(ErrorMessage ="Telefon Numarası Zorunludur")]
        public string? Phone { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Şifre Zorunludur")]
        public string? Password { get; set; }

        public bool IsAdmin { get; set; } = false;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
        public DateTime UpdateAt { get; set; }
        public int UpdatedBy { get; set; }

        // Navigation
        public ICollection<Message>? SentMessages { get; set; }
        public ICollection<Message>? ReceivedMessages { get; set; }
    }
}
