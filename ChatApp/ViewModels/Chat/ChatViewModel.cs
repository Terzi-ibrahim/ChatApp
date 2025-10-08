using ChatApp.Models;

namespace ChatApp.ViewModels.Chat
{
    public class ChatViewModel
    {
        public int CurrentUserId { get; set; } 
        public List<MemberDto> Members { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
    }

  
    public class MemberDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = "/img/user.jpg";
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; } = 0;
    }
}
