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
            new Abone { IsimSoyisim = "Emin Altan", TcKimlikNo = 12345678901, AboneNo = 100001, Telefon = 5551112233, FaturaId = 11111, Adres = "İzmir" },
            new Abone { IsimSoyisim = "Emir Alp Altan", TcKimlikNo = 23456789012, AboneNo = 100002, Telefon = 5552223344, FaturaId = 22222, Adres = "Aydın" },
            new Abone { IsimSoyisim = "Ahmet Mehmet", TcKimlikNo = 34567890123, AboneNo = 100003, Telefon = 5553334455, FaturaId = 33333, Adres = "Didim" },
            new Abone { IsimSoyisim = "Ali Veli", TcKimlikNo = 45678901234, AboneNo = 100004, Telefon = 5554445566, FaturaId = 44444, Adres = "Ankara" },
            new Abone { IsimSoyisim = "Ayşe Fatma", TcKimlikNo = 56789012345, AboneNo = 100005, Telefon = 5555556677, FaturaId = 55555, Adres = "Samsun" }
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
            var sessionData = string.IsNullOrEmpty(sessionDataStr) ? new ChatSessionData() : JsonConvert.DeserializeObject<ChatSessionData>(sessionDataStr) ?? new ChatSessionData();

            // Hibrit Model: Backend doğrulama + AI konuşma
            var (isValid, validationMessage, updatedSessionData) = await ValidateUserInput(scenario, sessionData, input.Message);
            
            // Session data'yı güncelle
            sessionData = updatedSessionData;
            HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));

            // AI'dan doğal konuşma yanıtı al
            string aiReply = await GetAIResponse(scenario, sessionData, input.Message, history, isValid, validationMessage);
            
            // History'yi güncelle
            history += $"User: {input.Message}\nBot: {aiReply}\n";
            HttpContext.Session.SetString("ChatHistory", history);

            // Konuşma geçmişini kaydet
            SaveChatHistory(history);

            return Json(new { reply = aiReply, history });
        }

        private async Task<(bool isValid, string validationMessage, ChatSessionData sessionData)> ValidateUserInput(string scenario, ChatSessionData sessionData, string userMessage)
        {
            // Serbest Sohbet için özel işlem
            if (scenario == "SerbestSohbet")
            {
                if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                {
                    sessionData.IsimSoyisim = userMessage.Trim();
                    return (true, "SERBEST_SOHBET_BASLADI", sessionData);
                }
                return (true, "SERBEST_SOHBET_DEVAM", sessionData);
            }

            // Standart akış doğrulaması
            var validationResult = await ValidateStandardFlow(scenario, sessionData, userMessage);
            return validationResult;
        }

        private async Task<(bool isValid, string validationMessage, ChatSessionData sessionData)> ValidateStandardFlow(string scenario, ChatSessionData sessionData, string userMessage)
        {
            // 1. Adım: Ad Soyad doğrulaması
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
            {
                sessionData.IsimSoyisim = userMessage.Trim();
                var abone = OrnekAboneler.FirstOrDefault(a => a.IsimSoyisim.Equals(sessionData.IsimSoyisim, StringComparison.OrdinalIgnoreCase));
                if (abone == null)
                {
                    sessionData.IsimSoyisim = string.Empty;
                    return (false, "AD_SOYAD_BULUNAMADI", sessionData);
                }
                return (true, "AD_SOYAD_DOGRULANDI", sessionData);
            }

            // 2. Adım: T.C. Kimlik doğrulaması
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
            {
                if (userMessage.Length == 11 && long.TryParse(userMessage, out var tcKimlik))
                {
                    var abone = OrnekAboneler.FirstOrDefault(a => 
                        a.IsimSoyisim.Equals(sessionData.IsimSoyisim, StringComparison.OrdinalIgnoreCase) && 
                        a.TcKimlikNo == tcKimlik);
                    
                    if (abone == null)
                    {
                        return (false, "TC_KIMLIK_YANLIS", sessionData);
                    }
                    
                    sessionData.TcKimlikNo = tcKimlik.ToString();
                    return (true, "TC_KIMLIK_DOGRULANDI", sessionData);
                }
                else
                {
                    return (false, "TC_KIMLIK_FORMAT_HATASI", sessionData);
                }
            }

            // 3. Adım: Abone No doğrulaması
            if (string.IsNullOrEmpty(sessionData.AboneNo))
            {
                if (userMessage.Length >= 6 && int.TryParse(userMessage, out var aboneNo))
                {
                    var abone = OrnekAboneler.FirstOrDefault(a => 
                        a.IsimSoyisim.Equals(sessionData.IsimSoyisim, StringComparison.OrdinalIgnoreCase) && 
                        a.TcKimlikNo.ToString() == sessionData.TcKimlikNo && 
                        a.AboneNo == aboneNo);
                    
                    if (abone == null)
                    {
                        return (false, "ABONE_NO_YANLIS", sessionData);
                    }
                    
                    sessionData.AboneNo = aboneNo.ToString();
                    return (true, "ABONE_DOGRULANDI", sessionData);
                }
                else
                {
                    return (false, "ABONE_NO_FORMAT_HATASI", sessionData);
                }
            }

            // 4. Adım: Senaryo özel doğrulamaları
            return await ValidateScenarioSpecificInput(scenario, sessionData, userMessage);
        }

        private Task<(bool isValid, string validationMessage, ChatSessionData sessionData)> ValidateScenarioSpecificInput(string scenario, ChatSessionData sessionData, string userMessage)
        {
            var result = scenario switch
            {
                "FaturaOdemesi" => ValidateFaturaOdemesi(sessionData, userMessage),
                "AbonelikIptali" => ValidateAbonelikIptali(sessionData, userMessage),
                "FaturaItirazi" => ValidateFaturaItirazi(sessionData, userMessage),
                "ElektrikArizasi" => ValidateElektrikArizasi(sessionData, userMessage),
                "Diger" => (true, "SERBEST_KONUSMA", sessionData),
                _ => (true, "SERBEST_KONUSMA", sessionData)
            };
            
            return Task.FromResult(result);
        }

        private (bool isValid, string validationMessage, ChatSessionData sessionData) ValidateFaturaOdemesi(ChatSessionData sessionData, string userMessage)
        {
            if (string.IsNullOrEmpty(sessionData.FaturaId))
            {
                if (userMessage.Length == 5 && int.TryParse(userMessage, out var faturaId))
                {
                    sessionData.FaturaId = faturaId.ToString();
                    return (true, "FATURA_ID_KAYDEDILDI", sessionData);
                }
                else
                {
                    return (false, "FATURA_ID_FORMAT_HATASI", sessionData);
                }
            }
            return (true, "FATURA_ODEMESI_TAMAMLANDI", sessionData);
        }

        private (bool isValid, string validationMessage, ChatSessionData sessionData) ValidateAbonelikIptali(ChatSessionData sessionData, string userMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IptalSebebi))
            {
                sessionData.IptalSebebi = userMessage.Trim();
                return (true, "IPTAL_SEBEBI_KAYDEDILDI", sessionData);
            }
            return (true, "ABONELIK_IPTALI_TAMAMLANDI", sessionData);
        }

        private (bool isValid, string validationMessage, ChatSessionData sessionData) ValidateFaturaItirazi(ChatSessionData sessionData, string userMessage)
        {
            if (string.IsNullOrEmpty(sessionData.FaturaId))
            {
                if (userMessage.Length == 5 && int.TryParse(userMessage, out var faturaId))
                {
                    sessionData.FaturaId = faturaId.ToString();
                    return (true, "ITIRAZ_FATURA_ID_KAYDEDILDI", sessionData);
                }
                else
                {
                    return (false, "FATURA_ID_FORMAT_HATASI", sessionData);
                }
            }
            
            if (string.IsNullOrEmpty(sessionData.ItirazSebebi))
            {
                sessionData.ItirazSebebi = userMessage.Trim();
                return (true, "ITIRAZ_SEBEBI_KAYDEDILDI", sessionData);
            }
            return (true, "FATURA_ITIRAZI_TAMAMLANDI", sessionData);
        }

        private (bool isValid, string validationMessage, ChatSessionData sessionData) ValidateElektrikArizasi(ChatSessionData sessionData, string userMessage)
        {
            if (string.IsNullOrEmpty(sessionData.Adres))
            {
                sessionData.Adres = userMessage.Trim();
                return (true, "ADRES_KAYDEDILDI", sessionData);
            }
            
            if (string.IsNullOrEmpty(sessionData.ArizaAciklamasi))
            {
                sessionData.ArizaAciklamasi = userMessage.Trim();
                return (true, "ARIZA_ACIKLAMASI_KAYDEDILDI", sessionData);
            }
            return (true, "ELEKTRIK_ARIZASI_TAMAMLANDI", sessionData);
        }

        private async Task<string> GetAIResponse(string scenario, ChatSessionData sessionData, string userMessage, string history, bool isValid, string validationMessage)
        {
            // AI için context oluştur
            var context = CreateAIContext(scenario, sessionData, userMessage, isValid, validationMessage);
            
            // AI'dan yanıt al
            var aiPrompt = $"{context}\n\nKullanıcı: {userMessage}\n\nSen:";
            var aiReply = await GptService.GetReplyAsync(aiPrompt, scenario);
            
            return aiReply;
        }

        private string CreateAIContext(string scenario, ChatSessionData sessionData, string userMessage, bool isValid, string validationMessage)
        {
            var context = $"Sen bir elektrik abonelik işlemleri asistanısın. Senaryo: {scenario}.\n\n";
            
            // Mevcut durum bilgileri
            context += "MEVCUT DURUM:\n";
            if (!string.IsNullOrEmpty(sessionData.IsimSoyisim))
                context += $"- Ad Soyad: {sessionData.IsimSoyisim}\n";
            if (!string.IsNullOrEmpty(sessionData.TcKimlikNo))
                context += $"- T.C. Kimlik: {sessionData.TcKimlikNo}\n";
            if (!string.IsNullOrEmpty(sessionData.AboneNo))
                context += $"- Abone No: {sessionData.AboneNo}\n";
            if (!string.IsNullOrEmpty(sessionData.FaturaId))
                context += $"- Fatura ID: {sessionData.FaturaId}\n";
            if (!string.IsNullOrEmpty(sessionData.Adres))
                context += $"- Adres: {sessionData.Adres}\n";
            if (!string.IsNullOrEmpty(sessionData.ArizaAciklamasi))
                context += $"- Arıza Açıklaması: {sessionData.ArizaAciklamasi}\n";
            if (!string.IsNullOrEmpty(sessionData.ItirazSebebi))
                context += $"- İtiraz Sebebi: {sessionData.ItirazSebebi}\n";
            if (!string.IsNullOrEmpty(sessionData.IptalSebebi))
                context += $"- İptal Sebebi: {sessionData.IptalSebebi}\n";
            
            context += $"\nDOĞRULAMA SONUCU: {(isValid ? "BAŞARILI" : "HATALI")}\n";
            
            // Validation message'ı AI için daha anlaşılır hale getir
            var aiValidationMessage = GetAIValidationMessage(validationMessage, scenario, sessionData);
            context += $"SON ADIM: {aiValidationMessage}\n\n";
            
            // Senaryo özel talimatlar
            context += GetScenarioInstructions(scenario, sessionData, isValid, validationMessage);
            
            return context;
        }

        private string GetAIValidationMessage(string validationMessage, string scenario, ChatSessionData sessionData)
        {
            return validationMessage switch
            {
                "SERBEST_SOHBET_BASLADI" => "Kullanıcı adını verdi, serbest sohbet başlat",
                "SERBEST_SOHBET_DEVAM" => "Serbest sohbet devam ediyor",
                "AD_SOYAD_DOGRULANDI" => "Ad soyad doğrulandı, T.C. Kimlik isteniyor",
                "AD_SOYAD_BULUNAMADI" => "Ad soyad bulunamadı, tekrar iste",
                "TC_KIMLIK_DOGRULANDI" => "T.C. Kimlik doğrulandı, Abone No isteniyor",
                "TC_KIMLIK_YANLIS" => "T.C. Kimlik yanlış, tekrar iste",
                "TC_KIMLIK_FORMAT_HATASI" => "T.C. Kimlik format hatası, 11 haneli olmalı",
                "ABONE_DOGRULANDI" => "Abone doğrulandı, senaryo özel adıma geç",
                "ABONE_NO_YANLIS" => "Abone No yanlış, tekrar iste",
                "ABONE_NO_FORMAT_HATASI" => "Abone No format hatası, en az 6 haneli olmalı",
                "FATURA_ID_KAYDEDILDI" => "Fatura ID kaydedildi, işlem tamamlandı",
                "FATURA_ID_FORMAT_HATASI" => "Fatura ID format hatası, 5 haneli olmalı",
                "IPTAL_SEBEBI_KAYDEDILDI" => "İptal sebebi kaydedildi, işlem tamamlandı",
                "ITIRAZ_FATURA_ID_KAYDEDILDI" => "İtiraz Fatura ID kaydedildi, itiraz sebebi isteniyor",
                "ITIRAZ_SEBEBI_KAYDEDILDI" => "İtiraz sebebi kaydedildi, işlem tamamlandı",
                "ADRES_KAYDEDILDI" => "Adres kaydedildi, arıza açıklaması isteniyor",
                "ARIZA_ACIKLAMASI_KAYDEDILDI" => "Arıza açıklaması kaydedildi, işlem tamamlandı",
                "FATURA_ODEMESI_TAMAMLANDI" => "Fatura ödemesi tamamlandı",
                "ABONELIK_IPTALI_TAMAMLANDI" => "Abonelik iptali tamamlandı",
                "FATURA_ITIRAZI_TAMAMLANDI" => "Fatura itirazı tamamlandı",
                "ELEKTRIK_ARIZASI_TAMAMLANDI" => "Elektrik arızası tamamlandı",
                "SERBEST_KONUSMA" => "Serbest konuşma modu",
                _ => validationMessage
            };
        }

        private string GetScenarioInstructions(string scenario, ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            switch (scenario)
            {
                case "FaturaOdemesi":
                    return GetFaturaOdemesiInstructions(sessionData, isValid, validationMessage);
                
                case "AbonelikIptali":
                    return GetAbonelikIptaliInstructions(sessionData, isValid, validationMessage);
                
                case "FaturaItirazi":
                    return GetFaturaItiraziInstructions(sessionData, isValid, validationMessage);
                
                case "ElektrikArizasi":
                    return GetElektrikArizasiInstructions(sessionData, isValid, validationMessage);
                
                case "Diger":
                    return GetDigerInstructions(sessionData, isValid, validationMessage);
                
                case "SerbestSohbet":
                    return GetSerbestSohbetInstructions(sessionData, isValid, validationMessage);
                
                default:
                    return "Genel konuşma yapabilirsin.";
            }
        }

        private string GetFaturaOdemesiInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Kullanıcıdan ad soyadını iste. Nazik ve profesyonel ol.";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "Kullanıcıdan T.C. Kimlik numarasını iste (11 haneli).";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Kullanıcıdan abone numarasını iste (6 haneli).";
            
            // Abone doğrulandıktan sonra fatura ID iste
            if (!string.IsNullOrEmpty(sessionData.AboneNo) && string.IsNullOrEmpty(sessionData.FaturaId))
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var aboneName = abone?.IsimSoyisim ?? "Sayın Müşterimiz";
                return $"Hoş geldiniz {aboneName}! Fatura ödemesi işleminiz için ödemek istediğiniz faturanın ID'sini öğrenebilir miyim? (5 haneli)";
            }
            
            if (string.IsNullOrEmpty(sessionData.FaturaId))
                return "Kullanıcıdan ödemek istediği faturanın ID'sini iste (5 haneli).";
            
            return "Fatura ödemesi işlemi tamamlandı. Teşekkür et ve başka bir konuda yardım edebileceğini belirt.";
        }

        private string GetAbonelikIptaliInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Kullanıcıdan ad soyadını iste. Nazik ve profesyonel ol.";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "Kullanıcıdan T.C. Kimlik numarasını iste (11 haneli).";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Kullanıcıdan abone numarasını iste (6 haneli).";
            
            // Abone doğrulandıktan sonra iptal sebebini iste
            if (!string.IsNullOrEmpty(sessionData.AboneNo) && string.IsNullOrEmpty(sessionData.IptalSebebi))
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var aboneName = abone?.IsimSoyisim ?? "Sayın Müşterimiz";
                return $"Hoş geldiniz {aboneName}! Abonelik iptali işleminiz için iptal sebebinizi öğrenebilir miyim? Size alternatif çözümler de sunabilirim.";
            }
            
            if (string.IsNullOrEmpty(sessionData.IptalSebebi))
                return "Kullanıcıdan iptal sebebini öğren. Anlayışlı ol ve alternatif çözümler önerebilirsin.";
            
            return "Abonelik iptali işlemi tamamlandı. İptal sebebini anladığını belirt ve başka bir konuda yardım edebileceğini söyle.";
        }

        private string GetFaturaItiraziInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Kullanıcıdan ad soyadını iste. Nazik ve profesyonel ol.";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "Kullanıcıdan T.C. Kimlik numarasını iste (11 haneli).";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Kullanıcıdan abone numarasını iste (6 haneli).";
            
            // Abone doğrulandıktan sonra fatura ID iste
            if (!string.IsNullOrEmpty(sessionData.AboneNo) && string.IsNullOrEmpty(sessionData.FaturaId))
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var aboneName = abone?.IsimSoyisim ?? "Sayın Müşterimiz";
                return $"Hoş geldiniz {aboneName}! Fatura itirazı işleminiz için itiraz etmek istediğiniz faturanın ID'sini öğrenebilir miyim? (5 haneli)";
            }
            
            if (string.IsNullOrEmpty(sessionData.FaturaId))
                return "Kullanıcıdan itiraz etmek istediği faturanın ID'sini iste (5 haneli).";
            
            if (string.IsNullOrEmpty(sessionData.ItirazSebebi))
                return "Kullanıcıdan itiraz sebebini öğren. Dikkatle dinle ve anlayışlı ol.";
            
            return "Fatura itirazı işlemi tamamlandı. İtirazını aldığını belirt ve inceleme sürecini açıkla.";
        }

        private string GetElektrikArizasiInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Kullanıcıdan ad soyadını iste. Nazik ve profesyonel ol.";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "Kullanıcıdan T.C. Kimlik numarasını iste (11 haneli).";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Kullanıcıdan abone numarasını iste (6 haneli).";
            
            // Abone doğrulandıktan sonra adres iste
            if (!string.IsNullOrEmpty(sessionData.AboneNo) && string.IsNullOrEmpty(sessionData.Adres))
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var aboneName = abone?.IsimSoyisim ?? "Sayın Müşterimiz";
                return $"Hoş geldiniz {aboneName}! Elektrik arızası bildirimi için adresinizi öğrenebilir miyim? Arıza yerini belirlemek için gerekli.";
            }
            
            if (string.IsNullOrEmpty(sessionData.Adres))
                return "Kullanıcıdan adresini iste. Arıza yerini belirlemek için gerekli.";
            
            if (string.IsNullOrEmpty(sessionData.ArizaAciklamasi))
                return "Kullanıcıdan arıza açıklamasını iste. Detaylı bilgi al ve hemen müdahale edeceğini belirt.";
            
            return "Elektrik arızası kaydı tamamlandı. Özür dile, hemen ekip göndereceğini belirt ve tahmini süre ver.";
        }

        private string GetDigerInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Kullanıcıdan ad soyadını iste. Nazik ve profesyonel ol.";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "Kullanıcıdan T.C. Kimlik numarasını iste (11 haneli).";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Kullanıcıdan abone numarasını iste (6 haneli).";
            
            return "Abone doğrulaması tamamlandı. Size nasıl yardımcı olabilirim? Genel soruları yanıtla ve gerekirse diğer departmanlara yönlendir.";
        }

        private string GetSerbestSohbetInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Kullanıcıdan adını öğren ve serbest sohbet başlat. Samimi ve yardımsever ol.";
            
            return "Serbest sohbet modundasın. Kullanıcı ile doğal bir şekilde konuş. Elektrik abonelik konularında da yardım edebilirsin.";
        }

        private void SaveChatHistory(string history)
        {
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
        }

        [HttpGet]
        public IActionResult Start(string scenario)
        {
            // Senaryo session'a kaydedilsin
            HttpContext.Session.SetString("Scenario", scenario ?? "Diger");
            
            // Session data'yı sıfırla
            HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(new ChatSessionData()));
            
            // Chat geçmişini sıfırla ve uygun karşılama mesajı ile başlat
            string welcome = scenario switch
            {
                "FaturaOdemesi" => "Merhaba! Fatura ödemesi işlemleriniz için size yardımcı olacağım. Öncelikle ad soyadınızı öğrenebilir miyim?",
                "AbonelikIptali" => "Merhaba! Abonelik iptali işlemleriniz için size yardımcı olacağım. Öncelikle ad soyadınızı öğrenebilir miyim?",
                "FaturaItirazi" => "Merhaba! Fatura itirazı işlemleriniz için size yardımcı olacağım. Öncelikle ad soyadınızı öğrenebilir miyim?",
                "ElektrikArizasi" => "Merhaba! Elektrik arızası bildirimi için size yardımcı olacağım. Öncelikle ad soyadınızı öğrenebilir miyim?",
                "Diger" => "Merhaba! Size nasıl yardımcı olabilirim? Öncelikle ad soyadınızı öğrenebilir miyim?",
                "SerbestSohbet" => "Merhaba! Ben AI destekli abonelik asistanınız. Size nasıl yardımcı olabilirim? Öncelikle adınızı öğrenebilir miyim?",
                _ => "Merhaba! Size nasıl yardımcı olabilirim? Öncelikle ad soyadınızı öğrenebilir miyim?"
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
                string firstBotMessage = "Merhaba! Hangi işlem için size yardımcı olabilirim? Fatura ödemesi, abonelik iptali, fatura itirazı, elektrik arızası veya diğer konularda destek verebilirim.";
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