using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Models
{
    public class Message
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Bir alıcı seçin.")]
        public int RecipientId { get; set; }

        [ForeignKey(nameof(RecipientId))]
        public User? Recipient { get; set; }

        [Required(ErrorMessage = "Gönderici zorunludur.")]
        public int SenderId { get; set; }

        [ForeignKey(nameof(SenderId))]
        public User? Sender { get; set; }

        [Required(ErrorMessage = "Mesaj boş olamaz")]
        [MaxLength(500)]
        public string? Messages { get; set; }

        public string? ImageUrl { get; set; }

        public bool  isRead { get; set; }
        public bool isDelete { get; set; } = false;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
        public DateTime UpdateAt { get; set; }
        public int UpdatedBy { get; set; }

    }
}
