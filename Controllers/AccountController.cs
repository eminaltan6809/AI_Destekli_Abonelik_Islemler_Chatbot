using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Claims;

namespace AI_Destekli_Abonelik_Chatbot.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string email, string fullName, string tcKimlik, string aboneNo, string? password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(tcKimlik) || string.IsNullOrWhiteSpace(aboneNo) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Tüm alanları doldurmanız gerekmektedir.";
                return View();
            }
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // Ek bilgileri UserClaims olarak ekle
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", fullName));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("TcKimlik", tcKimlik));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("AboneNo", aboneNo));
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = string.Join(" ", result.Errors.Select(e => e.Description));
            return View();
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");
            ViewBag.Error = "Geçersiz giriş.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            // Chat geçmişini temizle
            HttpContext.Session.Remove("ChatHistory");
            return RedirectToAction("Chat", "Chat");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // Geliştirme/test için tüm kullanıcıları ve chat kayıtlarını silen endpoint
        [HttpPost]
        public async Task<IActionResult> DeleteAllUsersAndChats()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                await _userManager.DeleteAsync(user);
            }
            // Chat kayıtlarını sil
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AI_Destekli_Abonelik_Chatbot.Models.AppDbContext>();
                db.ChatHistories.RemoveRange(db.ChatHistories);
                await db.SaveChangesAsync();
            }
            return Content("Tüm kullanıcılar ve chat kayıtları silindi.");
        }
    }
} 