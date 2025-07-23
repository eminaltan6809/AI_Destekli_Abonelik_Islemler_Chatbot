using Microsoft.AspNetCore.Mvc;
using AI_Destekli_Abonelik_Chatbot.Models;
using Newtonsoft.Json;
using AI_Destekli_Abonelik_Chatbot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;

namespace AI_Destekli_Abonelik_Chatbot.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public ChatController(AppDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Giriş zorunluluğu kaldırıldı
            base.OnActionExecuting(context);
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Message))
                return Json(new { reply = "Lütfen bir mesaj giriniz." });

            var history = HttpContext.Session.GetString("ChatHistory") ?? "";

            // Kullanıcı giriş yapmamışsa login/register işlemlerini chatbox üzerinden yap
            if (!User.Identity.IsAuthenticated)
            {
                var msg = input.Message.Trim();
                if (msg.StartsWith("giriş:", StringComparison.OrdinalIgnoreCase))
                {
                    // giriş: email, şifre
                    var parts = msg.Substring(6).Split(',');
                    if (parts.Length != 2)
                    {
                        history += "Bot: Lütfen 'giriş: email, şifre' formatında yazınız.\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = "Lütfen 'giriş: email, şifre' formatında yazınız.", history });
                    }
                    var email = parts[0].Trim();
                    var password = parts[1].Trim();
                    var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
                    if (result.Succeeded)
                    {
                        history += "Bot: Giriş başarılı! Hangi işlemi yapmak istiyorsunuz? 1- Fatura Ödemesi, 2- Abonelik İptali, 3- Fatura İtirazı, 4- Elektrik Arızası, 5- Diğer\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = "Giriş başarılı! Hangi işlemi yapmak istiyorsunuz? 1- Fatura Ödemesi, 2- Abonelik İptali, 3- Fatura İtirazı, 4- Elektrik Arızası, 5- Diğer", history });
                    }
                    history += "Bot: Giriş başarısız. Lütfen bilgilerinizi kontrol edin.\n";
                    HttpContext.Session.SetString("ChatHistory", history);
                    return Json(new { reply = "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.", history });
                }
                else if (msg.StartsWith("kayıt:", StringComparison.OrdinalIgnoreCase))
                {
                    // kayıt: email, ad soyad, tc kimlik, abone no, şifre
                    var parts = msg.Substring(6).Split(',');
                    if (parts.Length != 5)
                    {
                        history += "Bot: Lütfen 'kayıt: email, ad soyad, tc kimlik, abone no, şifre' formatında yazınız.\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = "Lütfen 'kayıt: email, ad soyad, tc kimlik, abone no, şifre' formatında yazınız.", history });
                    }
                    var email = parts[0].Trim();
                    var fullName = parts[1].Trim();
                    var tcKimlik = parts[2].Trim();
                    var aboneNo = parts[3].Trim();
                    var password = parts[4].Trim();
                    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(tcKimlik) || string.IsNullOrWhiteSpace(aboneNo) || string.IsNullOrWhiteSpace(password))
                    {
                        history += "Bot: Tüm alanları doldurmanız gerekmektedir.\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = "Tüm alanları doldurmanız gerekmektedir.", history });
                    }
                    var user = new IdentityUser { UserName = email, Email = email };
                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", fullName));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("TcKimlik", tcKimlik));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("AboneNo", aboneNo));
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        history += "Bot: Kayıt başarılı! Hangi işlemi yapmak istiyorsunuz? 1- Fatura Ödemesi, 2- Abonelik İptali, 3- Fatura İtirazı, 4- Elektrik Arızası, 5- Diğer\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = "Kayıt başarılı! Hangi işlemi yapmak istiyorsunuz? 1- Fatura Ödemesi, 2- Abonelik İptali, 3- Fatura İtirazı, 4- Elektrik Arızası, 5- Diğer", history });
                    }
                    var errorMsg = string.Join(" ", result.Errors.Select(e => e.Description));
                    history += $"Bot: {errorMsg}\n";
                    HttpContext.Session.SetString("ChatHistory", history);
                    return Json(new { reply = errorMsg, history });
                }
                else
                {
                    history += "Bot: Hoş geldiniz! Giriş yapmak için 'giriş: email, şifre' veya kayıt olmak için 'kayıt: email, ad soyad, tc kimlik, abone no, şifre' yazınız.\n";
                    HttpContext.Session.SetString("ChatHistory", history);
                    return Json(new { reply = "Hoş geldiniz! Giriş yapmak için 'giriş: email, şifre' veya kayıt olmak için 'kayıt: email, ad soyad, tc kimlik, abone no, şifre' yazınız.", history });
                }
            }

            // Kullanıcı giriş yaptıysa normal chat akışı
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
                            return Json(new { reply = bilgiAdimi, history });
                        }
                    }
                    else
                    {
                        bilgiAdimi = "Lütfen T.C. Kimlik numaranızı giriniz (11 haneli).";
                        history += $"Bot: {bilgiAdimi}\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = bilgiAdimi, history });
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
                        return Json(new { reply = bilgiAdimi, history });
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
            return Json(new { reply, history });
        }

        [HttpGet]
        public IActionResult Chat()
        {
            // Chat geçmişini her zaman göster
            string initialHistory = HttpContext.Session.GetString("ChatHistory");
            if (string.IsNullOrEmpty(initialHistory))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    string firstBotMessage = "Hoş geldiniz! Giriş yapmak için 'giriş: email, şifre' veya kayıt olmak için 'kayıt: email, ad soyad, tc kimlik, abone no, şifre' yazınız.";
                    initialHistory = $"Bot: {firstBotMessage}\n";
                }
                else
                {
                    // Giriş yapılmışsa geçmişi 'Hangi işlemi yapmak istiyorsunuz?' mesajı ile başlat
                    string firstBotMessage = "Hangi işlemi yapmak istiyorsunuz? 1- Fatura Ödemesi, 2- Abonelik İptali, 3- Fatura İtirazı, 4- Elektrik Arızası, 5- Diğer";
                    initialHistory = $"Bot: {firstBotMessage}\n";
                }
                HttpContext.Session.SetString("ChatHistory", initialHistory);
            }
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