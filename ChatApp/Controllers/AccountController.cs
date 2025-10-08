using ChatApp.Data;
using ChatApp.Hubs;
using ChatApp.Models;
using ChatApp.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    public class AccountController(AppDbContext context, IHubContext<ActiveUsersHub> hubContext) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly IHubContext<ActiveUsersHub> _hubContext =hubContext;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]  
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Email == model.Email || u.UserName == model.UserName);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Bu e-posta veya kullanıcı adı zaten kayıtlı.");
                return View(model);
            }

            
            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                UserName = model.UserName,
                Email = model.Email,
                Phone = model.Phone,
                Password = model.Password, 
                IsActive = false,         
                IsAdmin = false,
                CreateAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            // 🔹 Claims direkt login yap
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            };

            var identity = new ClaimsIdentity(claims, "login");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);


            return RedirectToAction("Index", "Home");

        }


        [HttpGet]
        public IActionResult Login()
        {
           
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            

            return View(); 
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u =>
                (u.Email == model.Identifier ||
                 u.Phone == model.Identifier ||
                 u.UserName == model.Identifier)
                && u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()), 
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(2)
                });

            user.IsActive = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            int activeCount = _context.Users.Count(u => u.IsActive);
            await _hubContext.Clients.All.SendAsync("UpdateActiveUserCount", activeCount);

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user != null)
            {
                user.IsActive = false;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            int activeCount = _context.Users.Count(u => u.IsActive);
            await _hubContext.Clients.All.SendAsync("UpdateActiveUserCount", activeCount);


            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }



        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

           
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

          
            var model = new ProfileViewModel
            {
                Id = user.id,
                Name = user.Name,
                Surname = user.Surname,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.Phone
            };

            return View(model);
         
        }

        [HttpPost]
        public async  Task<IActionResult> Profile( ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kullanıcı ID’sini Claims’ten tekrar al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            // DB’deki kaydı bul
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Güncelleme
            user.Name = model.Name;
            user.Surname = model.Surname;
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Phone = model.PhoneNumber;

            // Kaydet
            _context.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Profil başarıyla güncellendi!";



            return View();
        }

    }
}
