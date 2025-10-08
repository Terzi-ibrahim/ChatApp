namespace ChatApp.Models
{
    public class Notification
    {
        public int id { get; set; }
        public int UserId { get; set; } 
        public User? User { get; set; }
        public bool isread { get; set; }
        public DateTime UpdateAt { get; set; }
        public int UpdatedBy { get; set; }

    }
}
