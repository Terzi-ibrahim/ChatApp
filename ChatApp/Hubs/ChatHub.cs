using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Data;

namespace ChatApp.Hubs
{
    public class ChatHub(AppDbContext context) : Hub
    {
        private readonly AppDbContext _context = context;


        public async Task SendMessage(int recipientId, string message)
        {
            var senderId = int.Parse(Context.UserIdentifier ?? "0");
            Console.WriteLine($"SendMessage called: senderId={senderId}, recipientId={recipientId}, message={message}");

            if (senderId == 0)
            {
                throw new Exception("UserIdentifier null, authentication problemi var.");
            }

            var msg = new Message
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Messages = message,
                isRead = false,
                CreateAt = DateTime.UtcNow,
                CreatedBy = senderId,
                UpdatedBy = senderId,
                UpdateAt = DateTime.UtcNow
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            await Clients.User(recipientId.ToString()).SendAsync("ReceiveMessage", new
            {
                senderId = senderId,
                message = message,
                createAt = msg.CreateAt
            });

            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                senderId = senderId,
                message = message,
                createAt = msg.CreateAt
            });
        }


    }

}
