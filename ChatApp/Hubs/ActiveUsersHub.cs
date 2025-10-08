using ChatApp.Data;
using Microsoft.AspNetCore.SignalR;


namespace ChatApp.Hubs
{
    public class ActiveUsersHub(AppDbContext context): Hub
    {
        private readonly AppDbContext _context = context;
   
        public async Task UserLoggedIn()
        {
            int activeCout = await GetActiveUserCount();
            await Clients.All.SendAsync("UpdateActiveUserCount", activeCout);
        }
        private Task<int> GetActiveUserCount()
        {
      
            int count = _context.Users.Count(u => u.IsActive);
            return Task.FromResult(count);
        }
    }
}
