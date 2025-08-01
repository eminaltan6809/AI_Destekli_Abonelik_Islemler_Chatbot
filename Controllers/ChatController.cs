using Microsoft.AspNetCore.Mvc;
using AI_Destekli_Abonelik_Chatbot.Models;
using Newtonsoft.Json;
using AI_Destekli_Abonelik_Chatbot.Services;
using System.Collections.Generic;

namespace AI_Destekli_Abonelik_Chatbot.Controllers
{
    public class ChatController : Controller
    {
        // Sabit abone listesi - Abonelik iptali senaryoları için güncellenmiş
        public static List<Abone> OrnekAboneler = new List<Abone>
        {
            // 1. Kullanıcı: Sorunsuz abonelik sonlandırma (herhangi bir sıkıntı yok)
            new Abone 
            { 
                IsimSoyisim = "Emin Altan",
                TcKimlikNo = 12345678901,
                AboneNo = 1000000001,
                Telefon = 5551112233,
                FaturaId = 11111,
                Adres = "İzmir, Konak Mahallesi, No:123",
                BabaAdi = "Ahmet",
                DogumTarihi = new DateTime(2004, 6, 15),
                SozlesmeHesapNo = "1061234567",
                IbanNumarasi = "TR120006200000000123456789",
                GuvenceBedeli = 500.00m,
                GuncelBorc = 0.00m,
                AbonelikDurumu = "Normal",
                TuzelAbonelik = false,
                VefatDurumu = false,
                SayacSeriNo = "S123456789",
                AbonelikBaslangicTarihi = new DateTime(2020, 1, 1),
                AktifAbonelik = true,
                AbonelikTipi = "Konut"
            },
            
            // 2. Kullanıcı: Borçlu abonelik (borç var)
            new Abone 
            { 
                IsimSoyisim = "Emir Altan",
                TcKimlikNo = 23456789012,
                AboneNo = 1000000002,
                Telefon = 5552223344,
                FaturaId = 22222,
                Adres = "Aydın, Efeler Mahallesi, No:456",
                BabaAdi = "Mustafa",
                DogumTarihi = new DateTime(2016, 3, 3),
                SozlesmeHesapNo = "1062345678",
                IbanNumarasi = "TR120006200000000234567890",
                GuvenceBedeli = 750.00m,
                GuncelBorc = 812.47m,
                AbonelikDurumu = "Normal",
                TuzelAbonelik = false,
                VefatDurumu = false,
                SayacSeriNo = "S234567890",
                AbonelikBaslangicTarihi = new DateTime(2019, 6, 1),
                AktifAbonelik = true,
                AbonelikTipi = "Konut"
            },
            
            // 3. Kullanıcı: Vefat eden kişi aboneliği
            new Abone 
            { 
                IsimSoyisim = "Ali Veli", 
                TcKimlikNo = 34567890123, 
                AboneNo = 1000000003, 
                Telefon = 5553334455, 
                FaturaId = 33333, 
                Adres = "Didim, Altınkum Mahallesi, No:789",
                BabaAdi = "Hasan",
                DogumTarihi = new DateTime(1955, 8, 22),
                SozlesmeHesapNo = "1063456789",
                IbanNumarasi = "TR120006200000000345678901",
                GuvenceBedeli = 300.00m,
                GuncelBorc = 0.00m,
                AbonelikDurumu = "Normal",
                TuzelAbonelik = false,
                VefatDurumu = true,
                VarisBilgisi = "Tek varis",
                SayacSeriNo = "S345678901",
                AbonelikBaslangicTarihi = new DateTime(2018, 3, 15),
                AktifAbonelik = true,
                AbonelikTipi = "Konut",
                OzelDurumNotu = "Vefat eden kişi aboneliği"
            },
            
            // 4. Kullanıcı: Şirketsel abonelik (tüzel kişi)
            new Abone 
            { 
                IsimSoyisim = "Altan Teknoloji A.Ş.", 
                TcKimlikNo = 12345678999,
                AboneNo = 1000000004, 
                Telefon = 5554445566, 
                FaturaId = 44444, 
                Adres = "İstanbul, Levent Mahallesi, Büyükdere Caddesi No:123",
                BabaAdi = "",
                DogumTarihi = new DateTime(2000, 1, 1),
                SozlesmeHesapNo = "1064567890",
                IbanNumarasi = "TR120006200000000456789012",
                GuvenceBedeli = 1500.00m,
                GuncelBorc = 0.00m,
                AbonelikDurumu = "Normal",
                TuzelAbonelik = true,
                TuzelKurumAdi = "Altan Teknoloji A.Ş.",
                YetkiliKisiAdi = "Altan Yönetici",
                VefatDurumu = false,
                SayacSeriNo = "S456789012",
                AbonelikBaslangicTarihi = new DateTime(2021, 5, 10),
                AktifAbonelik = true,
                AbonelikTipi = "Ticari",
                OzelDurumNotu = "Tüzel kişi aboneliği"
            },
            
            // 5. Kullanıcı: Sistemsel hata durumu aboneliği
            new Abone 
            { 
                IsimSoyisim = "Atakan Kağan",
                TcKimlikNo = 12345678900,
                AboneNo = 1000000005,
                Telefon = 5555555566,
                FaturaId = 55555,
                Adres = "Ankara, Çankaya Mahallesi, No:321",
                BabaAdi = "Ahmet",
                DogumTarihi = new DateTime(1990, 12, 6),
                SozlesmeHesapNo = "1065555555",
                IbanNumarasi = "TR120006200000000555555555",
                GuvenceBedeli = 400.00m,
                GuncelBorc = 0.00m,
                AbonelikDurumu = "Normal",
                TuzelAbonelik = false,
                VefatDurumu = false,
                SayacSeriNo = "S555555555",
                AbonelikBaslangicTarihi = new DateTime(2022, 8, 15),
                AktifAbonelik = true,
                AbonelikTipi = "Konut",
                OzelDurumNotu = "Sistemsel hata durumu aboneliği"
            }
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

            // Tüzel abonelik tespit edildiğinde otomatik canlı destek yönlendirmesi
            if (validationMessage == "TUZEL_ABONELIK_TESPIT_EDILDI")
            {
                // Önce tüzel abonelik mesajını ver
                string tuzelAbonelikMesaji = "Bu abonelik, tüzel kişi (şirket) adına kayıtlı görünüyor. Tüzel abonelik işlemlerinde, yalnızca yetkili kişilerle ve özel doğrulama adımlarıyla işlem yapılabilmektedir. Bu işlem dijital asistan aracılığıyla gerçekleştirilememektedir. Sizi hemen canlı destek ekibimize aktarıyorum. Lütfen hatta kalın, bağlantınız sağlanıyor...";
                
                // Tüzel abonelik durumunu session'a kaydet
                sessionData.TuzelAbonelik = "Evet";
                HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
                
                // History'yi güncelle
                history += $"Siz: {input.Message}\nAsistan: {tuzelAbonelikMesaji}\n";
                HttpContext.Session.SetString("ChatHistory", history);
                
                // Konuşma geçmişini kaydet
                SaveChatHistory(history);
                
                // 3 saniye sonra canlı destek senaryosuna geç
                return Json(new { reply = tuzelAbonelikMesaji, history, redirectToCanliDestek = true, delay = 3000 });
            }

            // Vefat durumu tespit edildiğinde otomatik canlı destek yönlendirmesi
            if (validationMessage == "VEFAT_DURUMU_TESPIT_EDILDI")
            {
                // Önce vefat durumu mesajını ver
                string vefatMesaji = "Başınız sağ olsun. Böyle bir durumda işlemleri hassasiyetle ele alıyoruz. Bu işlem, özel bir prosedür gerektiriyor. Şu aşamada sizi canlı destek ekibimize aktarıyorum. Onlar, durumunuzu detaylıca dinleyip uygun yönlendirmeyi sağlayacaktır. Lütfen hatta kalın, birazdan bağlanacaksınız.";
                
                // Vefat durumunu session'a kaydet
                sessionData.VefatDurumu = "Evet";
                HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
                
                // History'yi güncelle
                history += $"Siz: {input.Message}\nAsistan: {vefatMesaji}\n";
                HttpContext.Session.SetString("ChatHistory", history);
                
                // Konuşma geçmişini kaydet
                SaveChatHistory(history);
                
                // 3 saniye sonra canlı destek senaryosuna geç
                return Json(new { reply = vefatMesaji, history, redirectToCanliDestek = true, delay = 3000 });
            }

            // Sistemsel hata durumu tespit edildiğinde otomatik canlı destek yönlendirmesi
            if (validationMessage == "SISTEMSEL_HATA_TESPIT_EDILDI")
            {
                // Önce sistemsel hata mesajını ver
                string sistemselHataMesaji = "Şu anda sistemimizde teknik bir aksaklık oluştu. Ne yazık ki işlemi tamamlayamıyorum. Sizi daha fazla bekletmeden canlı destek ekibimize aktarıyorum. Oradaki arkadaşlarımız işleminizi hemen inceleyip yardımcı olacaklardır. Lütfen hatta kalın, birazdan bağlanacaksınız.";
                
                // Sistemsel hata durumunu session'a kaydet
                sessionData.SistemselHata = "Evet";
                HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
                
                // History'yi güncelle
                history += $"Siz: {input.Message}\nAsistan: {sistemselHataMesaji}\n";
                HttpContext.Session.SetString("ChatHistory", history);
                
                // Konuşma geçmişini kaydet
                SaveChatHistory(history);
                
                // 3 saniye sonra canlı destek senaryosuna geç
                return Json(new { reply = sistemselHataMesaji, history, redirectToCanliDestek = true, delay = 3000 });
            }

            // AI'dan doğal konuşma yanıtı al (vefat durumu hariç)
            string aiReply = await GetAIResponse(scenario, sessionData, input.Message, history, isValid, validationMessage);
            
            // History'yi güncelle
            history += $"Siz: {input.Message}\nAsistan: {aiReply}\n";
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

            // Canlı Destek için özel işlem
            if (scenario == "CanliDestek")
            {
                if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                {
                    var isimSoyisim = userMessage.Trim();
                    
                    // Basit ad soyad kontrolü (en az 2 kelime olmalı)
                    var kelimeler = isimSoyisim.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (kelimeler.Length < 2)
                    {
                        return (false, "CANLI_DESTEK_ISIM_FORMAT_HATASI", sessionData);
                    }
                    
                    // İsim soyisimde sadece harf ve boşluk olmalı
                    if (!isimSoyisim.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                    {
                        return (false, "CANLI_DESTEK_ISIM_KARAKTER_HATASI", sessionData);
                    }
                    
                    sessionData.IsimSoyisim = isimSoyisim;
                    return (true, "CANLI_DESTEK_BASLADI", sessionData);
                }
                return (true, "CANLI_DESTEK_DEVAM", sessionData);
            }

            // Abonelik İptali için özel akış
            if (scenario == "AbonelikIptali")
            {
                var validationResult = ValidateAbonelikIptali(sessionData, userMessage);
                return await Task.FromResult(validationResult);
            }

            // Diğer senaryolar için standart akış doğrulaması
            var standardValidationResult = await ValidateStandardFlow(scenario, sessionData, userMessage);
            return standardValidationResult;
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
                    
                    // Tüzel abonelik kontrolü
                    if (abone.TuzelAbonelik)
                    {
                        sessionData.AboneNo = aboneNo.ToString();
                        sessionData.TuzelAbonelik = "Evet";
                        return (true, "TUZEL_ABONELIK_TESPIT_EDILDI", sessionData);
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
                "CanliDestek" => (true, "CANLI_DESTEK_DEVAM", sessionData),
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
            // 1. Adım: Abone sahibi mi sorusu
            if (string.IsNullOrEmpty(sessionData.AboneSahibiMi))
            {
                var response = userMessage.Trim().ToLower();
                if (response.Contains("evet") || response.Contains("yes"))
                {
                    sessionData.AboneSahibiMi = "Evet";
                    sessionData.IslemAdimi = "AboneNo";
                    return (true, "ABONE_SAHIBI_EVET", sessionData);
                }
                else if (response.Contains("hayır") || response.Contains("no") || response.Contains("değil"))
                {
                    sessionData.AboneSahibiMi = "Hayır";
                    return (true, "ABONE_SAHIBI_HAYIR", sessionData);
                }
                else if (response.Contains("vefat") || response.Contains("öldü") || response.Contains("ölüm"))
                {
                    sessionData.AboneSahibiMi = "Vefat";
                    sessionData.VefatDurumu = "Evet";
                    return (true, "VEFAT_DURUMU_TESPIT_EDILDI", sessionData);
                }
                else if (response.Contains("şirket") || response.Contains("firma") || response.Contains("tüzel"))
                {
                    sessionData.AboneSahibiMi = "Şirket";
                    sessionData.TuzelAbonelik = "Evet";
                    return (true, "ABONE_SAHIBI_SIRKET", sessionData);
                }
                else
                {
                    return (false, "ABONE_SAHIBI_BELIRSIZ", sessionData);
                }
            }

            // 2. Adım: Abone numarası (sadece Evet durumunda)
            if (sessionData.AboneSahibiMi == "Evet" && sessionData.IslemAdimi == "AboneNo" && string.IsNullOrEmpty(sessionData.AboneNo))
            {
                if (userMessage.Length == 10 && long.TryParse(userMessage, out var aboneNo))
                {
                    // Abone numarasına göre abone bilgilerini kontrol et
                    var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo == aboneNo);
                    
                    if (abone == null)
                    {
                        return (false, "ABONE_NO_BULUNAMADI", sessionData);
                    }
                    
                    // Tüzel abonelik kontrolü
                    if (abone.TuzelAbonelik)
                    {
                        sessionData.AboneNo = aboneNo.ToString();
                        sessionData.TuzelAbonelik = "Evet";
                        return (true, "TUZEL_ABONELIK_TESPIT_EDILDI", sessionData);
                    }
                    
                    sessionData.AboneNo = aboneNo.ToString();
                    sessionData.IslemAdimi = "IsimSoyisim";
                    return (true, "ABONE_NO_DOGRULANDI", sessionData);
                }
                else
                {
                    return (false, "ABONE_NO_FORMAT_HATASI", sessionData);
                }
            }

            // 2. Adım: İsim Soyisim
            if (sessionData.IslemAdimi == "IsimSoyisim" && string.IsNullOrEmpty(sessionData.IsimSoyisim))
            {
                var isimSoyisim = userMessage.Trim();
                
                // Abone numarası ile isim tutarlılığını kontrol et
                var abone = OrnekAboneler.FirstOrDefault(a => 
                    a.AboneNo.ToString() == sessionData.AboneNo &&
                    a.IsimSoyisim.Equals(isimSoyisim, StringComparison.OrdinalIgnoreCase));
                
                if (abone == null)
                {
                    return (false, "ABONE_NO_ISIM_ESLESMIYOR", sessionData);
                }
                
                sessionData.IsimSoyisim = isimSoyisim;
                sessionData.IslemAdimi = "TcKimlikNo";
                return (true, "ISIM_SOYISIM_KAYDEDILDI", sessionData);
            }

            // 3. Adım: T.C. Kimlik No
            if (sessionData.IslemAdimi == "TcKimlikNo" && string.IsNullOrEmpty(sessionData.TcKimlikNo))
            {
                if (userMessage.Length == 11 && long.TryParse(userMessage, out var tcKimlik))
                {
                    // Abone numarası, isim ve T.C. kimlik tutarlılığını kontrol et
                    var abone = OrnekAboneler.FirstOrDefault(a => 
                        a.AboneNo.ToString() == sessionData.AboneNo &&
                        a.IsimSoyisim.Equals(sessionData.IsimSoyisim, StringComparison.OrdinalIgnoreCase) &&
                        a.TcKimlikNo == tcKimlik);
                    
                    if (abone == null)
                    {
                        return (false, "ABONE_NO_ISIM_TC_ESLESMIYOR", sessionData);
                    }
                    
                    sessionData.TcKimlikNo = tcKimlik.ToString();
                    sessionData.IslemAdimi = "DogumTarihi";
                    return (true, "TC_KIMLIK_KAYDEDILDI", sessionData);
                }
                else
                {
                    return (false, "TC_KIMLIK_FORMAT_HATASI", sessionData);
                }
            }

            // 4. Adım: Doğum Tarihi
            if (sessionData.IslemAdimi == "DogumTarihi" && string.IsNullOrEmpty(sessionData.DogumTarihi))
            {
                if (DateTime.TryParseExact(userMessage, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var dogumTarihi))
                {
                    // Tüm bilgileri kontrol et
                    var abone = OrnekAboneler.FirstOrDefault(a => 
                        a.AboneNo.ToString() == sessionData.AboneNo &&
                        a.IsimSoyisim.Equals(sessionData.IsimSoyisim, StringComparison.OrdinalIgnoreCase) &&
                        a.TcKimlikNo.ToString() == sessionData.TcKimlikNo &&
                        a.DogumTarihi.ToString("dd/MM/yyyy") == dogumTarihi.ToString("dd/MM/yyyy"));
                    
                    if (abone == null)
                    {
                        return (false, "BILGILER_ESLESMIYOR", sessionData);
                    }
                    
                    sessionData.DogumTarihi = dogumTarihi.ToString("dd/MM/yyyy");
                    sessionData.IslemAdimi = "BorcKontrolu";
                    return (true, "DOGUM_TARIHI_KAYDEDILDI", sessionData);
                }
                else
                {
                    return (false, "DOGUM_TARIHI_FORMAT_HATASI", sessionData);
                }
            }

            // 5. Adım: Borç kontrolü
            if (sessionData.IslemAdimi == "BorcKontrolu" && string.IsNullOrEmpty(sessionData.BorcKontrolu))
            {
                // Abone bilgilerini kontrol et
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                if (abone != null)
                {
                    // Sistemsel hata durumu kontrolü (Tuba Hatipoğlu aboneliği)
                    if (abone.AboneNo == 1000000005)
                    {
                        sessionData.BorcKontrolu = "SistemselHata";
                        return (true, "SISTEMSEL_HATA_TESPIT_EDILDI", sessionData);
                    }
                    
                    if (abone.GuncelBorc > 0)
                    {
                        sessionData.BorcKontrolu = "BorçVar";
                        return (true, "BORC_VAR", sessionData);
                    }
                    else
                    {
                        sessionData.BorcKontrolu = "BorçYok";
                        sessionData.IslemAdimi = "KesilmeGunu";
                        return (true, "BORC_YOK", sessionData);
                    }
                }
                else
                {
                    return (false, "ABONE_BULUNAMADI", sessionData);
                }
            }

            // 6. Adım: Kesilme günü seçimi
            if (sessionData.IslemAdimi == "KesilmeGunu" && string.IsNullOrEmpty(sessionData.EnerjiKesmeTarihi))
            {
                if (int.TryParse(userMessage, out var kesilmeGunu))
                {
                    if (kesilmeGunu >= 1 && kesilmeGunu <= 5)
                    {
                        var kesilmeTarihi = DateTime.Today.AddDays(kesilmeGunu);
                        sessionData.EnerjiKesmeTarihi = kesilmeTarihi.ToString("dd/MM/yyyy");
                        sessionData.IslemAdimi = "IbanBilgisi";
                        return (true, "KESILME_GUNU_KAYDEDILDI", sessionData);
                    }
                    else
                    {
                        return (false, "KESILME_GUNU_ARALIK_HATASI", sessionData);
                    }
                }
                else
                {
                    return (false, "KESILME_GUNU_FORMAT_HATASI", sessionData);
                }
            }

            // 7. Adım: IBAN bilgisi
            if (sessionData.IslemAdimi == "IbanBilgisi" && string.IsNullOrEmpty(sessionData.IbanNumarasi))
            {
                if (userMessage.StartsWith("TR") && userMessage.Length == 26)
                {
                    sessionData.IbanNumarasi = userMessage;
                    sessionData.IslemAdimi = "Tamamlandi";
                    return (true, "IBAN_KAYDEDILDI", sessionData);
                }
                else
                {
                    return (false, "IBAN_FORMAT_HATASI", sessionData);
                }
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
            
            // Abonelik iptali için yeni alanlar
            if (!string.IsNullOrEmpty(sessionData.AboneSahibiMi))
                context += $"- Abone Sahibi Mi: {sessionData.AboneSahibiMi}\n";
            if (!string.IsNullOrEmpty(sessionData.SozlesmeHesapNo))
                context += $"- Sözleşme Hesap No: {sessionData.SozlesmeHesapNo}\n";
            if (!string.IsNullOrEmpty(sessionData.BabaAdi))
                context += $"- Baba Adı: {sessionData.BabaAdi}\n";
            if (!string.IsNullOrEmpty(sessionData.DogumTarihi))
                context += $"- Doğum Tarihi: {sessionData.DogumTarihi}\n";
            if (!string.IsNullOrEmpty(sessionData.BorcKontrolu))
                context += $"- Borç Kontrolü: {sessionData.BorcKontrolu}\n";
            if (!string.IsNullOrEmpty(sessionData.EnerjiKesmeTarihi))
                context += $"- Enerji Kesme Tarihi: {sessionData.EnerjiKesmeTarihi}\n";
            if (!string.IsNullOrEmpty(sessionData.IbanNumarasi))
                context += $"- IBAN: {sessionData.IbanNumarasi}\n";
            if (!string.IsNullOrEmpty(sessionData.IslemAdimi))
                context += $"- İşlem Adımı: {sessionData.IslemAdimi}\n";
            if (!string.IsNullOrEmpty(sessionData.VefatDurumu))
                context += $"- Vefat Durumu: {sessionData.VefatDurumu}\n";
            if (!string.IsNullOrEmpty(sessionData.TuzelAbonelik))
                context += $"- Tüzel Abonelik: {sessionData.TuzelAbonelik}\n";
            if (!string.IsNullOrEmpty(sessionData.SistemselHata))
                context += $"- Sistemsel Hata: {sessionData.SistemselHata}\n";
            
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
                "CANLI_DESTEK_BASLADI" => "Kullanıcı adını verdi, canlı destek başlat",
                "CANLI_DESTEK_DEVAM" => "Canlı destek devam ediyor",
                "CANLI_DESTEK_ISIM_FORMAT_HATASI" => "Ad soyad en az 2 kelime olmalı",
                "CANLI_DESTEK_ISIM_KARAKTER_HATASI" => "Ad soyad sadece harf içermeli",
                "AD_SOYAD_DOGRULANDI" => "Ad soyad doğrulandı, T.C. Kimlik isteniyor",
                "AD_SOYAD_BULUNAMADI" => "Ad soyad bulunamadı, tekrar iste",
                "TC_KIMLIK_DOGRULANDI" => "T.C. Kimlik doğrulandı, Abone No isteniyor",
                "TC_KIMLIK_YANLIS" => "T.C. Kimlik yanlış, tekrar iste",
                "ABONE_DOGRULANDI" => "Abone doğrulandı, senaryo özel adıma geç",
                "ABONE_NO_YANLIS" => "Abone No yanlış, tekrar iste",
                "FATURA_ID_KAYDEDILDI" => "Fatura ID kaydedildi, işlem tamamlandı",
                "FATURA_ID_FORMAT_HATASI" => "Fatura ID format hatası, 5 haneli olmalı",
                "ITIRAZ_FATURA_ID_KAYDEDILDI" => "İtiraz Fatura ID kaydedildi, itiraz sebebi isteniyor",
                "ITIRAZ_SEBEBI_KAYDEDILDI" => "İtiraz sebebi kaydedildi, işlem tamamlandı",
                "ADRES_KAYDEDILDI" => "Adres kaydedildi, arıza açıklaması isteniyor",
                "ARIZA_ACIKLAMASI_KAYDEDILDI" => "Arıza açıklaması kaydedildi, işlem tamamlandı",
                "FATURA_ODEMESI_TAMAMLANDI" => "Fatura ödemesi tamamlandı",
                "ABONELIK_IPTALI_TAMAMLANDI" => "Abonelik iptali tamamlandı",
                "FATURA_ITIRAZI_TAMAMLANDI" => "Fatura itirazı tamamlandı",
                "ELEKTRIK_ARIZASI_TAMAMLANDI" => "Elektrik arızası tamamlandı",
                "SERBEST_KONUSMA" => "Serbest konuşma modu",
                
                // Abonelik iptali yeni mesajları
                "ABONE_SAHIBI_EVET" => "Abone sahibi evet dedi, abone numarası isteniyor",
                "ABONE_SAHIBI_HAYIR" => "Abone sahibi hayır dedi, KVKK gereği işlem yapılamaz",
                "ABONE_SAHIBI_VEFAT" => "Abone sahibi vefat etti, canlı desteğe aktarım gerekli",
                "VEFAT_DURUMU_TESPIT_EDILDI" => "Vefat durumu tespit edildi, canlı desteğe aktarım",
                "ABONE_SAHIBI_SIRKET" => "Tüzel abonelik, canlı desteğe aktarım gerekli",
                "ABONE_SAHIBI_BELIRSIZ" => "Abone sahibi sorusu belirsiz, tekrar sor",
                "ABONE_NO_DOGRULANDI" => "Abone numarası doğrulandı, isim soyisim isteniyor",
                "ABONE_NO_FORMAT_HATASI" => "Abone numarası format hatası, 10 haneli olmalı",
                "ABONE_NO_BULUNAMADI" => "Abone numarası bulunamadı, lütfen kontrol edin",
                "TUZEL_ABONELIK_TESPIT_EDILDI" => "Tüzel abonelik tespit edildi, canlı desteğe aktarım",
                "SISTEMSEL_HATA_TESPIT_EDILDI" => "Sistemsel hata tespit edildi, canlı desteğe aktarım",
                "ISIM_SOYISIM_KAYDEDILDI" => "İsim soyisim kaydedildi, T.C. kimlik no isteniyor",
                "TC_KIMLIK_KAYDEDILDI" => "T.C. kimlik no kaydedildi, doğum tarihi isteniyor",
                "TC_KIMLIK_FORMAT_HATASI" => "T.C. kimlik no format hatası, 11 haneli olmalı",
                "DOGUM_TARIHI_KAYDEDILDI" => "Doğum tarihi kaydedildi, borç kontrolü yapılıyor",
                "DOGUM_TARIHI_FORMAT_HATASI" => "Doğum tarihi format hatası, GG/AA/YYYY formatında olmalı",
                "BILGILER_ESLESMIYOR" => "Girdiğiniz bilgiler sistemdeki kayıtlarla eşleşmiyor, lütfen kontrol edin",
                "ABONE_NO_ISIM_ESLESMIYOR" => "Abone numarası ile girdiğiniz isim soyisim eşleşmiyor, lütfen kontrol edin",
                "ABONE_NO_ISIM_TC_ESLESMIYOR" => "Abone numarası, isim soyisim ve T.C. kimlik numarası eşleşmiyor, lütfen kontrol edin",

                "BORC_VAR" => "Borç tespit edildi, borç tutarı bildiriliyor",
                "BORC_YOK" => "Borç yok, kesilme günü seçimi isteniyor",
                "ABONE_BULUNAMADI" => "Abone bulunamadı, abone numarasını kontrol et",
                "KESILME_GUNU_KAYDEDILDI" => "Kesilme günü kaydedildi, IBAN bilgisi isteniyor",
                "KESILME_GUNU_ARALIK_HATASI" => "Kesilme günü 1-5 arasında olmalı",
                "KESILME_GUNU_FORMAT_HATASI" => "Kesilme günü sayı formatında olmalı",
                "IBAN_KAYDEDILDI" => "IBAN kaydedildi, işlem tamamlanıyor",
                "IBAN_FORMAT_HATASI" => "IBAN format hatası, TR ile başlayan 26 haneli olmalı",
                
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
                
                case "CanliDestek":
                    return GetCanliDestekInstructions(sessionData, isValid, validationMessage);
                
                default:
                    return "Genel konuşma yapabilirsin.";
            }
        }

        private string GetFaturaOdemesiInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Ad soyadınızı öğrenebilir miyim?";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "T.C. Kimlik numaranızı öğrenebilir miyim? (11 haneli)";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Abone numaranızı öğrenebilir miyim? (6 haneli)";
            
            // Abone doğrulandıktan sonra fatura ID iste
            if (!string.IsNullOrEmpty(sessionData.AboneNo) && string.IsNullOrEmpty(sessionData.FaturaId))
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var aboneName = abone?.IsimSoyisim ?? "Sayın Müşterimiz";
                return $"Hoş geldiniz {aboneName}! Ödemek istediğiniz faturanın ID'sini öğrenebilir miyim? (5 haneli)";
            }
            
            if (string.IsNullOrEmpty(sessionData.FaturaId))
                return "Ödemek istediğiniz faturanın ID'sini öğrenebilir miyim? (5 haneli)";
            
            return "Fatura ödemesi işlemi tamamlandı. Teşekkür ederim. Başka bir konuda yardımcı olabilirim.";
        }

        private string GetAbonelikIptaliInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            // Vefat durumu kontrolü - özel mesaj döndür
            if (sessionData.AboneSahibiMi == "Vefat" || sessionData.VefatDurumu == "Evet")
            {
                return "Başınız sağ olsun. Böyle bir durumda işlemleri hassasiyetle ele alıyoruz. Bu işlem, özel bir prosedür gerektiriyor. Şu aşamada sizi canlı destek ekibimize aktarıyorum. Onlar, durumunuzu detaylıca dinleyip uygun yönlendirmeyi sağlayacaktır. Lütfen hatta kalın, birazdan bağlanacaksınız.";
            }

            // 1. Adım: Abone sahibi mi sorusu
            if (string.IsNullOrEmpty(sessionData.AboneSahibiMi))
            {
                return "Merhaba! Ben Dijital Asistanınız! Size abonelik sonlandırma işleminizde yardımcı olacağım. İşleme başlamadan önce, bazı bilgileri teyit etmem gerekiyor. Öncelikle, aboneliğin sahibi siz misiniz?";
            }

            // 2. Adım: Abone sahibi hayır dedi
            if (sessionData.AboneSahibiMi == "Hayır")
            {
                return "Anlayışınız için teşekkür ederim. Bu işlem kişisel bilgilere ve abonelik haklarına yönelik olduğu için, sadece abonelik sahibi ya da sistemimizde kayıtlı yetkili kişi ile devam edebiliyoruz. Bu nedenle şu an için işlemi başlatamıyorum. En kısa sürede abone sahibiyle iletişime geçilmesini rica ederim. Başka bir konuda size yardımcı olabilirsem memnuniyetle buradayım.";
            }

            // 3. Adım: Vefat durumu (artık yukarıda kontrol ediliyor)
            // Bu kontrol kaldırıldı çünkü yukarıda özel kontrol eklendi

            // 4. Adım: Tüzel abonelik
            if (sessionData.AboneSahibiMi == "Şirket")
            {
                return "Bu abonelik, tüzel kişi (şirket) adına kayıtlı görünüyor. Tüzel abonelik işlemlerinde, yalnızca yetkili kişilerle ve özel doğrulama adımlarıyla işlem yapılabilmektedir. Bu işlem dijital asistan aracılığıyla gerçekleştirilememektedir. Sizi hemen canlı destek ekibimize aktarıyorum. Lütfen hatta kalın, bağlantınız sağlanıyor...";
            }

            // 5. Adım: Abone numarasından tüzel abonelik tespiti
            if (sessionData.TuzelAbonelik == "Evet")
            {
                return "Bu abonelik, tüzel kişi (şirket) adına kayıtlı görünüyor. Tüzel abonelikler yalnızca yetkili kişilerle ve özel doğrulama adımlarıyla işlem yapılabilmektedir. Bu işlem dijital asistan aracılığıyla gerçekleştirilememektedir. Sizi hemen canlı destek ekibimize aktarıyorum. Lütfen hatta kalın, bağlantınız sağlanıyor...";
            }

            // 5. Adım: Abone numarası
            if (sessionData.AboneSahibiMi == "Evet" && sessionData.IslemAdimi == "AboneNo" && string.IsNullOrEmpty(sessionData.AboneNo))
            {
                return "Teşekkür ederim. Devam edebilmem için lütfen 10 haneli abone numaranızı paylaşır mısınız?";
            }

            // 2. Adım: İsim Soyisim
            if (sessionData.IslemAdimi == "IsimSoyisim")
            {
                return "Teşekkür ederim. Şimdi ad soyadınızı öğrenebilir miyim? (Abone numaranızla eşleşen isim soyisim olmalıdır)";
            }

            // 3. Adım: T.C. Kimlik No
            if (sessionData.IslemAdimi == "TcKimlikNo")
            {
                return "T.C. Kimlik numaranızı öğrenebilir miyim? (11 haneli, abone numaranız ve isim soyisiminizle eşleşen)";
            }

            // 4. Adım: Doğum Tarihi
            if (sessionData.IslemAdimi == "DogumTarihi")
            {
                return "Doğum tarihinizi GG/AA/YYYY formatında öğrenebilir miyim?";
            }

            // 5. Adım: Borç kontrolü sonrası
            if (sessionData.BorcKontrolu == "BorçVar")
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var borcTutari = abone?.GuncelBorc.ToString("N2") ?? "0.00";
                return $"Bu abonelikle ilişkili ödenmemiş bir borç mevcut. Güncel borç tutarınız: {borcTutari} TL. Borcunuz ödendikten sonra abonelik sonlandırma işlemini tekrar başlatabiliriz. Ödemenizi mobil bankacılıktan, yüz-yüze ödeme kanalları ve N-Kolay Ödeme Merkezlerinden kolayca yapabilirsiniz. Şu anda işlemi devam ettiremiyorum. Ancak dilerseniz işlemi daha sonra kaldığınız yerden tekrar başlatabilirim. Yardımcı olabileceğim başka bir konu var mı?";
            }

            // 6. Adım: Kesilme günü seçimi
            if (sessionData.IslemAdimi == "KesilmeGunu")
            {
                return "Teşekkür ederim. Bilgileriniz başarıyla doğrulandı. Şimdi aboneliğinizin kaç gün içinde kesilmesini istiyorsunuz? (1-5 gün arası bir sayı giriniz)";
            }

            // 7. Adım: IBAN bilgisi
            if (sessionData.IslemAdimi == "IbanBilgisi")
            {
                return "Son olarak, güvence bedelinin iadesi için IBAN bilginizi alabilir miyim? (Lütfen başında TR olacak şekilde giriniz.)";
            }

            // 8. Adım: İşlem tamamlandı
            if (sessionData.IslemAdimi == "Tamamlandi")
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var aboneName = abone?.IsimSoyisim ?? "Sayın Müşterimiz";
                return $"IBAN bilginiz başarıyla kaydedildi. {sessionData.EnerjiKesmeTarihi} tarihinde aboneliğiniz kesilecek. Ardından son tüketim faturanızı oluşturacağız ve güvence bedelinizden düşerek sözleşmenizi resmi olarak sonlandıracağız. Kalan güvence bedeli tutarınız aboneliğiniz kesildikten sonra 5 iş günü içerisinde iletmiş olduğunuz IBAN numarasına yatırılacaktır. İşleminiz başarıyla başlatıldı. Sonlandırma işleminiz gerçekleştiğinde size SMS ile bilgilendirme sağlanacaktır. Başka bir konuda yardımcı olabilir miyim?";
            }

            return "Abonelik iptali işlemi devam ediyor. Size nasıl yardımcı olabilirim?";
        }

        private string GetFaturaItiraziInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Ad soyadınızı öğrenebilir miyim?";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "T.C. Kimlik numaranızı öğrenebilir miyim? (11 haneli)";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Abone numaranızı öğrenebilir miyim? (6 haneli)";
            
            // Abone doğrulandıktan sonra fatura ID iste
            if (!string.IsNullOrEmpty(sessionData.AboneNo) && string.IsNullOrEmpty(sessionData.FaturaId))
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var aboneName = abone?.IsimSoyisim ?? "Sayın Müşterimiz";
                return $"Hoş geldiniz {aboneName}! İtiraz etmek istediğiniz faturanın ID'sini öğrenebilir miyim? (5 haneli)";
            }
            
            if (string.IsNullOrEmpty(sessionData.FaturaId))
                return "İtiraz etmek istediğiniz faturanın ID'sini öğrenebilir miyim? (5 haneli)";
            
            if (string.IsNullOrEmpty(sessionData.ItirazSebebi))
                return "İtiraz sebebinizi öğrenebilir miyim?";
            
            return "İtirazınız kaydedildi. İnceleme sürecimiz başlatıldı. Teşekkür ederim.";
        }

        private string GetElektrikArizasiInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Ad soyadınızı öğrenebilir miyim?";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "T.C. Kimlik numaranızı öğrenebilir miyim? (11 haneli)";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Abone numaranızı öğrenebilir miyim? (6 haneli)";
            
            // Abone doğrulandıktan sonra adres iste
            if (!string.IsNullOrEmpty(sessionData.AboneNo) && string.IsNullOrEmpty(sessionData.Adres))
            {
                var abone = OrnekAboneler.FirstOrDefault(a => a.AboneNo.ToString() == sessionData.AboneNo);
                var aboneName = abone?.IsimSoyisim ?? "Sayın Müşterimiz";
                return $"Hoş geldiniz {aboneName}! Arıza yerini belirlemek için adresinizi öğrenebilir miyim?";
            }
            
            if (string.IsNullOrEmpty(sessionData.Adres))
                return "Arıza yerini belirlemek için adresinizi öğrenebilir miyim?";
            
            if (string.IsNullOrEmpty(sessionData.ArizaAciklamasi))
                return "Arıza detaylarını öğrenebilir miyim?";
            
            return "Arıza kaydınız alındı. Ekip en kısa sürede yola çıkacak. Teşekkür ederim.";
        }

        private string GetDigerInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Ad soyadınızı öğrenebilir miyim?";
            
            if (string.IsNullOrEmpty(sessionData.TcKimlikNo))
                return "T.C. Kimlik numaranızı öğrenebilir miyim? (11 haneli)";
            
            if (string.IsNullOrEmpty(sessionData.AboneNo))
                return "Abone numaranızı öğrenebilir miyim? (6 haneli)";
            
            return "Size nasıl yardımcı olabilirim?";
        }

        private string GetSerbestSohbetInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Adınızı öğrenebilir miyim?";
            
            return "Size nasıl yardımcı olabilirim?";
        }

        private string GetCanliDestekInstructions(ChatSessionData sessionData, bool isValid, string validationMessage)
        {
            if (string.IsNullOrEmpty(sessionData.IsimSoyisim))
                return "Ad soyadınızı öğrenebilir miyim?";
            
            return "Size nasıl yardımcı olabilirim?";
        }

        private string GetCanliDestekWelcomeMessage(ChatSessionData sessionData)
        {
            // Sistemsel hata durumu kontrolü
            if (sessionData.SistemselHata == "Evet")
            {
                return "Merhaba! Canlı destek hizmetine hoş geldiniz. Sistemsel bir hatadan ötürü buraya yönlendirildiniz. Size hemen yardımcı olacağım ve işleminizi tamamlayacağım. Öncelikle ad soyadınızı öğrenebilir miyim?";
            }
            
            // Vefat durumu kontrolü
            if (sessionData.VefatDurumu == "Evet")
            {
                return "Merhaba! Canlı destek hizmetine hoş geldiniz. Vefat durumu ile ilgili işlemleriniz için size özel olarak yardımcı olacağım. Bu hassas konuda gerekli tüm adımları birlikte atacağız. Öncelikle ad soyadınızı öğrenebilir miyim?";
            }
            
            // Tüzel abonelik kontrolü
            if (sessionData.TuzelAbonelik == "Evet")
            {
                return "Merhaba! Canlı destek hizmetine hoş geldiniz. Tüzel abonelik işlemleriniz için size özel olarak yardımcı olacağım. Bu işlemler için gerekli doğrulama adımlarını birlikte tamamlayacağız. Öncelikle ad soyadınızı öğrenebilir miyim?";
            }
            
            // Normal canlı destek
            return "Merhaba! Canlı destek hizmetine hoş geldiniz. Size nasıl yardımcı olabilirim? Öncelikle ad soyadınızı öğrenebilir miyim?";
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
            
            // Mevcut session data'yı al
            var sessionDataStr = HttpContext.Session.GetString("SessionData");
            var sessionData = string.IsNullOrEmpty(sessionDataStr) ? new ChatSessionData() : JsonConvert.DeserializeObject<ChatSessionData>(sessionDataStr) ?? new ChatSessionData();
            
            // Eğer yeni bir senaryo başlatılıyorsa (canlı destek değilse) session data'yı sıfırla
            if (scenario != "CanliDestek")
            {
                sessionData = new ChatSessionData();
                HttpContext.Session.SetString("SessionData", JsonConvert.SerializeObject(sessionData));
            }
            
            // Chat geçmişini sıfırla ve uygun karşılama mesajı ile başlat
            string welcome = scenario switch
            {
                "FaturaOdemesi" => "Merhaba! Fatura ödemesi işlemleriniz için size yardımcı olacağım. Öncelikle ad soyadınızı öğrenebilir miyim?",
                "AbonelikIptali" => "Merhaba! Ben Dijital Asistanınız! Size abonelik sonlandırma işleminizde yardımcı olacağım. İşleme başlamadan önce, bazı bilgileri teyit etmem gerekiyor. Öncelikle, aboneliğin sahibi siz misiniz?",
                "FaturaItirazi" => "Merhaba! Fatura itirazı işlemleriniz için size yardımcı olacağım. Öncelikle ad soyadınızı öğrenebilir miyim?",
                "ElektrikArizasi" => "Merhaba! Elektrik arızası bildirimi için size yardımcı olacağım. Öncelikle ad soyadınızı öğrenebilir miyim?",
                "Diger" => "Merhaba! Size nasıl yardımcı olabilirim? Öncelikle ad soyadınızı öğrenebilir miyim?",
                "SerbestSohbet" => "Merhaba! Ben AI destekli abonelik asistanınız. Size nasıl yardımcı olabilirim? Öncelikle adınızı öğrenebilir miyim?",
                "CanliDestek" => GetCanliDestekWelcomeMessage(sessionData),
                _ => "Merhaba! Size nasıl yardımcı olabilirim? Öncelikle ad soyadınızı öğrenebilir miyim?"
            };
            
            string initialHistory = $"Asistan: {welcome}\n";
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
                initialHistory = $"Asistan: {firstBotMessage}\n";
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