namespace ChatApp.ViewModels.Chat
{
    public class MessageDto
    {
        public int id { get; set; }
        public string Messages { get; set; }
        public int RecipientId { get; set; }
        public int SenderId { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
