using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;


namespace ChatApp.Controllers
{
    public class HomeController(ILogger<HomeController> logger, AppDbContext context) : Controller
    {
        private readonly ILogger<HomeController> _logger =logger;
        private readonly AppDbContext _context = context;

       
        public IActionResult Index()
        {
            int saat = DateTime.Now.Hour;
            ViewBag.selamlama = saat > 12 ? " Ýyi günler " : "  Günaydýn";
            ViewBag.saat= saat;

            int activeUserCount = _context.Users.Count(u => u.IsActive);

            // View'e gönder
            ViewBag.ActiveUserCount = activeUserCount;
            return View();
        }

      

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
