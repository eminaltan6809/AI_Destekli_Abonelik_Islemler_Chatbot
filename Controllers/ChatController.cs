using Microsoft.AspNetCore.Mvc;
using AI_Destekli_Abonelik_Chatbot.Models;
using Newtonsoft.Json;
using AI_Destekli_Abonelik_Chatbot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AI_Destekli_Abonelik_Chatbot.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _db;
        public ChatController(AppDbContext db) { _db = db; }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (context.Controller is Controller controller)
                    controller.TempData["LoginRequired"] = true;
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            base.OnActionExecuting(context);
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Message))
                return Json(new { reply = "Lütfen bir mesaj giriniz." });
            var history = HttpContext.Session.GetString("ChatHistory") ?? "";
            var scenario = HttpContext.Session.GetString("Scenario") ?? "Diger";
            var sessionDataStr = HttpContext.Session.GetString("SessionData");
            var sessionData = string.IsNullOrEmpty(sessionDataStr) ? new ChatSessionData() : JsonConvert.DeserializeObject<ChatSessionData>(sessionDataStr);
            string bilgiAdimi = string.Empty;
            if (scenario == "FaturaOdemesi" || scenario == "AbonelikIptali")
            {
                if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                {
                    if (input.Message.Length == 11 && long.TryParse(input.Message, out _))
                    {
                        sessionData.TcKimlikNo = input.Message;
                        if (input.Message != "12345678901")
                        {
                            bilgiAdimi = "T.C. Kimlik numarası doğrulanamadı. Lütfen tekrar giriniz (ör: 12345678901).";
                            history += $"Bot: {bilgiAdimi}\n";
                            HttpContext.Session.SetString("ChatHistory", history);
                            return Json(new { reply = bilgiAdimi });
                        }
                    }
                    else
                    {
                        bilgiAdimi = "Lütfen T.C. Kimlik numaranızı giriniz (11 haneli).";
                        history += $"Bot: {bilgiAdimi}\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = bilgiAdimi });
                    }
                }
                else if (string.IsNullOrEmpty(sessionData.AboneNo))
                {
                    if (input.Message.Length >= 6 && input.Message.All(char.IsDigit))
                    {
                        sessionData.AboneNo = input.Message;
                    }
                    else
                    {
                        bilgiAdimi = "Lütfen abone numaranızı giriniz (en az 6 rakam).";
                        history += $"Bot: {bilgiAdimi}\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = bilgiAdimi });
                    }
                }
            }
            HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
            history += $"User: {input.Message}\n";
            var reply = await GptService.GetReplyAsync(history, scenario);
            history += $"Bot: {reply}\n";
            HttpContext.Session.SetString("ChatHistory", history);
            // Chat geçmişini veritabanına kaydet
            var userId = User.Identity != null && User.Identity.IsAuthenticated ? User.Identity.Name : HttpContext.Session.Id;
            _db.ChatHistories.Add(new ChatHistory
            {
                UserId = userId,
                Scenario = scenario,
                History = history
            });
            await _db.SaveChangesAsync();
            return Json(new { reply });
        }

        [HttpGet]
        public IActionResult Start(ChatScenario scenario)
        {
            // Senaryoya göre ilk mesajı belirle
            string firstBotMessage = scenario switch
            {
                ChatScenario.FaturaOdemesi => "Lütfen T.C. Kimlik numaranızı giriniz (11 haneli).",
                ChatScenario.AbonelikIptali => "Lütfen abone numaranızı giriniz (en az 6 rakam).",
                ChatScenario.FaturaItirazi => "Lütfen fatura numaranızı giriniz.",
                ChatScenario.ElektrikArizasi => "Lütfen adresinizi veya abone numaranızı giriniz.",
                _ => "Size nasıl yardımcı olabilirim?"
            };
            string initialHistory = $"Bot: {firstBotMessage}\n";
            HttpContext.Session.SetString("Scenario", scenario.ToString());
            HttpContext.Session.SetString("ChatHistory", initialHistory);
            ViewBag.Scenario = scenario;
            return View("Chat");
        }

        [HttpGet]
        public IActionResult MyHistory()
        {
            var userId = User.Identity != null && User.Identity.IsAuthenticated ? User.Identity.Name : HttpContext.Session.Id;
            var histories = _db.ChatHistories.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToList();
            return View(histories);
        }

        // Chat işlemleri ve GPT entegrasyonu burada olacak (devamında eklenecek)
    }
} 