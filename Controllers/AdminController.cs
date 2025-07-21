using Microsoft.AspNetCore.Mvc;
using AI_Destekli_Abonelik_Chatbot.Models;
using Newtonsoft.Json;

namespace AI_Destekli_Abonelik_Chatbot.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            // Şimdilik sessiondan mock chat geçmişi alalım (gerçek uygulamada veritabanı kullanılır)
            var chatHistory = HttpContext.Session.GetString("ChatHistory") ?? "";
            var scenario = HttpContext.Session.GetString("Scenario") ?? "";
            var sessionDataStr = HttpContext.Session.GetString("SessionData");
            var sessionData = string.IsNullOrEmpty(sessionDataStr) ? new ChatSessionData() : JsonConvert.DeserializeObject<ChatSessionData>(sessionDataStr);
            ViewBag.ChatHistory = chatHistory;
            ViewBag.Scenario = scenario;
            ViewBag.SessionData = sessionData;
            return View();
        }
    }
} 