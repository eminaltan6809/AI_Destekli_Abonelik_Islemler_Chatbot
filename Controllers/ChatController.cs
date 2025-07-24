using Microsoft.AspNetCore.Mvc;
using AI_Destekli_Abonelik_Chatbot.Models;
using Newtonsoft.Json;
using AI_Destekli_Abonelik_Chatbot.Services;
using System.Collections.Generic;

namespace AI_Destekli_Abonelik_Chatbot.Controllers
{
    public class ChatController : Controller
    {
        // Sabit abone listesi
        public static List<Abone> OrnekAboneler = new List<Abone>
        {
            new Abone { IsimSoyisim = "Emin Altan", TcKimlikNo = 12345678901, AboneNo = 100001, Telefon = 5551112233 },
            new Abone { IsimSoyisim = "Emir Alp Altan", TcKimlikNo = 23456789012, AboneNo = 100002, Telefon = 5552223344 },
            new Abone { IsimSoyisim = "Ahmet Mehmet", TcKimlikNo = 34567890123, AboneNo = 100003, Telefon = 5553334455 },
            new Abone { IsimSoyisim = "Ali Veli", TcKimlikNo = 45678901234, AboneNo = 100004, Telefon = 5554445566 },
            new Abone { IsimSoyisim = "Ayşe Fatma", TcKimlikNo = 56789012345, AboneNo = 100005, Telefon = 5555556677 }
        };

        // Konuşma geçmişi için statik liste
        public static List<ChatHistory> KonusmaGecmisi = new List<ChatHistory>();

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

            // Sıralı abone doğrulama akışı
            if (scenario != "SerbestSohbet")
            {
                // 1. Adım: İsim Soyisim
                if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                {
                    sessionData.IsimSoyisim = input.Message.Trim();
                    var abone = OrnekAboneler.FirstOrDefault(a => a.IsimSoyisim.Equals(sessionData.IsimSoyisim, StringComparison.OrdinalIgnoreCase));
                    if (abone == null)
                    {
                        bilgiAdimi = "Girilen isim ve soyisim ile eşleşen bir abone bulunamadı. Lütfen tekrar giriniz.";
                        sessionData.IsimSoyisim = string.Empty;
                        history += $"Bot: {bilgiAdimi}\n";
                        HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = bilgiAdimi, history });
                    }
                    else
                    {
                        bilgiAdimi = "Lütfen T.C. Kimlik numaranızı giriniz (11 haneli).";
                        history += $"Bot: {bilgiAdimi}\n";
                        HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = bilgiAdimi, history });
                    }
                }
                // 2. Adım: T.C. Kimlik
                else if (sessionData.TcKimlikNo == null || sessionData.TcKimlikNo == "")
                {
                    if (input.Message.Length == 11 && long.TryParse(input.Message, out var tcKimlik))
                    {
                        var abone = OrnekAboneler.FirstOrDefault(a => a.IsimSoyisim.Equals(sessionData.IsimSoyisim, StringComparison.OrdinalIgnoreCase) && a.TcKimlikNo == tcKimlik);
                        if (abone == null)
                        {
                            bilgiAdimi = "T.C. Kimlik numarası yanlış. Lütfen tekrar giriniz.";
                            history += $"Bot: {bilgiAdimi}\n";
                            HttpContext.Session.SetString("ChatHistory", history);
                            return Json(new { reply = bilgiAdimi, history });
                        }
                        sessionData.TcKimlikNo = tcKimlik.ToString();
                        bilgiAdimi = "Lütfen abone numaranızı giriniz (6 haneli).";
                        history += $"Bot: {bilgiAdimi}\n";
                        HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = bilgiAdimi, history });
                    }
                    else
                    {
                        bilgiAdimi = "T.C. Kimlik numarası 11 haneli olmalı ve sadece rakamlardan oluşmalı. Lütfen tekrar giriniz.";
                        history += $"Bot: {bilgiAdimi}\n";
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = bilgiAdimi, history });
                    }
                }
                // 3. Adım: Abone No
                else if (sessionData.AboneNo == null || sessionData.AboneNo == "")
                {
                    if (input.Message.Length >= 6 && int.TryParse(input.Message, out var aboneNo))
                    {
                        var abone = OrnekAboneler.FirstOrDefault(a => a.IsimSoyisim.Equals(sessionData.IsimSoyisim, StringComparison.OrdinalIgnoreCase) && a.TcKimlikNo.ToString() == sessionData.TcKimlikNo && a.AboneNo == aboneNo);
                        if (abone == null)
                        {
                            bilgiAdimi = "Abone numarası yanlış. Lütfen tekrar giriniz.";
                            history += $"Bot: {bilgiAdimi}\n";
                            HttpContext.Session.SetString("ChatHistory", history);
                            return Json(new { reply = bilgiAdimi, history });
                        }
                        sessionData.AboneNo = aboneNo.ToString();
                        bilgiAdimi = $"Hoş geldiniz, {abone.IsimSoyisim}! İşleminize devam edebilirsiniz.";
                        history += $"Bot: {bilgiAdimi}\n";
                        HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
                        HttpContext.Session.SetString("ChatHistory", history);
                        return Json(new { reply = bilgiAdimi, history });
                    }
                    else
                    {
                        bilgiAdimi = "Abone numarası en az 6 haneli olmalı ve sadece rakamlardan oluşmalı. Lütfen tekrar giriniz.";
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

            // Konuşma geçmişini kaydet
            var sessionId = HttpContext.Session.Id;
            var existing = KonusmaGecmisi.FirstOrDefault(x => x.SessionId == sessionId);
            if (existing == null)
            {
                KonusmaGecmisi.Add(new ChatHistory { SessionId = sessionId, Messages = history, CreatedAt = DateTime.Now });
            }
            else
            {
                existing.Messages = history;
                existing.CreatedAt = DateTime.Now;
            }

            return Json(new { reply, history });
        }
        [HttpGet]
        public IActionResult Start(string scenario)
        {
            // Senaryo session'a kaydedilsin
            HttpContext.Session.SetString("Scenario", scenario ?? "Diger");
            // Chat geçmişini sıfırla ve uygun karşılama mesajı ile başlat
            string welcome = scenario switch
            {
                "FaturaOdemesi" => "Merhaba, Fatura ödemesi kısmına geldiniz. Devam etmek için lütfen ad soyadınızı giriniz.",
                "AbonelikIptali" => "Merhaba, Abonelik iptali kısmına geldiniz. Devam etmek için lütfen ad soyadınızı giriniz.",
                "FaturaItirazi" => "Merhaba, Fatura itirazı kısmına geldiniz. Devam etmek için lütfen ad soyadınızı giriniz.",
                "ElektrikArizasi" => "Merhaba, Elektrik arızası kısmına geldiniz. Devam etmek için lütfen ad soyadınızı giriniz.",
                "Diger" => "Merhaba, Diğer işlemler kısmına geldiniz. Devam etmek için lütfen ad soyadınızı giriniz.",
                "SerbestSohbet" => "Merhaba, serbest sohbet başlatıldı. Devam etmek için lütfen ad soyadınızı giriniz.",
                _ => "Merhaba, nasıl yardımcı olabilirim? Devam etmek için lütfen ad soyadınızı giriniz."
            };
            string initialHistory = $"Bot: {welcome}\n";
            HttpContext.Session.SetString("ChatHistory", initialHistory);
            ViewBag.Scenario = scenario;
            return View("Chat");
        }
        [HttpGet]
        public IActionResult Chat()
        {
            string initialHistory = HttpContext.Session.GetString("ChatHistory");
            if (string.IsNullOrEmpty(initialHistory))
            {
                string firstBotMessage = "Hangi işlemi yapmak istiyorsunuz? 1- Fatura Ödemesi, 2- Abonelik İptali, 3- Fatura İtirazı, 4- Elektrik Arızası, 5- Diğer";
                initialHistory = $"Bot: {firstBotMessage}\n";
                HttpContext.Session.SetString("ChatHistory", initialHistory);
            }
            return View("Chat");
        }

        [HttpGet]
        public IActionResult History()
        {
            var sessionId = HttpContext.Session.Id;
            var history = KonusmaGecmisi.FirstOrDefault(x => x.SessionId == sessionId)?.Messages ?? "Henüz bir konuşma yok.";
            ViewBag.History = history;
            return View();
        }
    }
} 