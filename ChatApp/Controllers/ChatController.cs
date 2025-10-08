using ChatApp.Data;
using ChatApp.Hubs;
using ChatApp.Models;
using ChatApp.ViewModels.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    public class ChatController(AppDbContext context, IHubContext<ChatHub> chatHub) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly IHubContext<ChatHub> _chatHubContext = chatHub;

        public IActionResult Index(int? userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

          
            var members = _context.Users
                .Where(u => u.id != currentUserId)
                .Select(u => new MemberDto
                {
                    Id = u.id,
                    Name = u.Name,      
                    LastMessage = _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.RecipientId == u.id) ||
                    (m.SenderId == u.id && m.RecipientId == currentUserId))
                    .OrderByDescending(m => m.CreateAt) 
                    .Select(m => m.Messages)
                    .FirstOrDefault() ?? "",

                    LastMessageTime = _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.RecipientId == u.id) ||
                    (m.SenderId == u.id && m.RecipientId == currentUserId)) 
                    .OrderByDescending(m => m.CreateAt) 
                    .Select(m => m.CreateAt)
                    .FirstOrDefault(),

                    UnreadCount = _context.Messages
                    .Count(m => m.SenderId == u.id && m.RecipientId == currentUserId && !m.isRead)
                })
                .ToList();


         
            List<Message> messages = new();
            if (userId.HasValue)
            {
                messages = _context.Messages
                    .Where(m => (m.SenderId == currentUserId && m.RecipientId == userId.Value) ||
                                (m.SenderId == userId.Value && m.RecipientId == currentUserId))
                    .OrderBy(m => m.CreateAt)
                    .ToList();
            }

            var model = new ChatViewModel
            {
                CurrentUserId = currentUserId,
                Members = members,
                Messages = messages
            };

            return View(model);
        }
        [HttpGet]
        public IActionResult GetMessages(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"GetMessages called. CurrentUserId={currentUserId}, userId={userId}");

            var messages = _context.Messages
                .Where(m => !m.isDelete &&
                ((m.SenderId == currentUserId && m.RecipientId == userId) ||
                (m.SenderId == userId && m.RecipientId == currentUserId)))
                .OrderBy(m => m.CreateAt)
                .Select(m => new
                {
                    senderId = m.SenderId,
                    message = m.Messages,
                    imageUrl = m.ImageUrl,
                    createAt = m.CreateAt
                })
                .ToList();


            return Json(messages);
        }


        [HttpPost]
        public IActionResult MarkAsRead(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"MarkAsRead called. CurrentUserId={currentUserId}, userId={userId}");

            var unreadMessages = _context.Messages
                .Where(m => m.SenderId == userId && m.RecipientId == currentUserId && !m.isRead)
                .ToList();

            unreadMessages.ForEach(m => m.isRead = true);
            _context.SaveChanges();

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] int recipientId, [FromForm] string? message, [FromForm] IFormFile? image)
        {
            try
            {
                var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (senderId == 0)
                    return Json(new { success = false, error = "Kullanıcı bilgisi alınamadı." });

                string? imageUrl = null;
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

                if (image != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);
                    imageUrl = "/uploads/" + fileName;
                }

                var msg = new Message
                {
                    SenderId = senderId,
                    RecipientId = recipientId,
                    Messages = message ?? "",
                    ImageUrl = imageUrl,
                    isRead = false,
                    CreateAt = DateTime.UtcNow,
                    CreatedBy = senderId,
                    UpdatedBy = senderId,
                    UpdateAt = DateTime.UtcNow
                };

                _context.Messages.Add(msg);
                await _context.SaveChangesAsync();

                await _chatHubContext.Clients.User(recipientId.ToString()).SendAsync("ReceiveMessage", new
                {
                    senderId,
                    message = msg.Messages,
                    imageUrl = msg.ImageUrl,
                    createAt = msg.CreateAt
                });

                await _chatHubContext.Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", new
                {
                    senderId,
                    message = msg.Messages,
                    imageUrl = msg.ImageUrl,
                    createAt = msg.CreateAt
                });

                return Json(new { success = true, senderId, message = msg.Messages, imageUrl = msg.ImageUrl, createAt = msg.CreateAt });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }




        [HttpPost]
        public async Task<IActionResult> ClearMessages(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.RecipientId == currentUserId) ||
                            (m.SenderId == currentUserId && m.RecipientId == userId))
                .ToListAsync();

            messages.ForEach(m => m.isDelete = true);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }




    }
}
