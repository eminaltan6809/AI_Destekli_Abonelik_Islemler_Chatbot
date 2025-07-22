using Microsoft.AspNetCore.Mvc;
using AI_Destekli_Abonelik_Chatbot.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AI_Destekli_Abonelik_Chatbot.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public AdminController(AppDbContext db, UserManager<IdentityUser> userManager) { _db = db; _userManager = userManager; }
        public IActionResult Index()
        {
            var histories = _db.ChatHistories.OrderByDescending(x => x.CreatedAt).ToList();
            var users = _userManager.Users.ToList();
            ViewBag.Users = users;
            return View(histories);
        }
    }
} 