using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

namespace AI_Destekli_Abonelik_Chatbot.Services
{
    public static class GptService
    {
        // Google AI Studio API Key (Gemini Pro)
        private static string? _apiKey;
        private static readonly string _endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=";

        public static void Initialize(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _apiKey = config["GoogleAI:ApiKey"];
        }

        public static async Task<string> GetReplyAsync(string prompt, string scenario)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Google AI Studio API anahtarı eksik. Lütfen yöneticinize başvurun.";
            
            if (string.IsNullOrWhiteSpace(prompt))
                prompt = "Kullanıcı henüz bir mesaj göndermedi.";
            
            if (string.IsNullOrWhiteSpace(scenario))
                scenario = "Diger";

            // Hibrit model için geliştirilmiş system prompt
            var systemPrompt = CreateSystemPrompt(scenario);
            var fullPrompt = $"{systemPrompt}\n\n{prompt}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[] {
                            new { text = fullPrompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 500
                }
            };

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_endpoint + _apiKey, content);
                var json = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    return $"Üzgünüm, şu anda teknik bir sorun yaşıyorum. Lütfen biraz sonra tekrar deneyiniz. (Hata: {response.StatusCode})";
                }
                
                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json) ?? new { };
                
                // Gemini Pro cevabı: obj.candidates[0].content.parts[0].text
                var aiResponse = obj?.candidates?[0]?.content?.parts?[0]?.text?.ToString() ?? "Üzgünüm, şu anda yanıt veremiyorum.";
                
                // Yanıtı temizle ve optimize et
                return CleanAndOptimizeResponse(aiResponse, scenario);
            }
            catch (Exception ex)
            {
                return $"Üzgünüm, şu anda bağlantı sorunu yaşıyorum. Lütfen biraz sonra tekrar deneyiniz. (Hata: {ex.Message})";
            }
        }

        private static string CreateSystemPrompt(string scenario)
        {
            var basePrompt = @"Sen Türkiye'de bir elektrik dağıtım şirketinin müşteri hizmetleri asistanısın. 

KİŞİLİK ÖZELLİKLERİN:
- Nazik, profesyonel ve yardımsever ol
- Türkçe konuş, samimi ama resmi dil kullan
- Müşteri memnuniyetini ön planda tut
- Sabırlı ol ve açıklamalarını net yap
- Gerektiğinde alternatif çözümler öner
- Empatik ol ve müşterinin durumunu anlamaya çalış

KONUŞMA TARZIN:
- 'Teşekkür ederim', 'Rica ederim' gibi nezaket ifadeleri kullan
- Kısa ve öz cevaplar ver, gereksiz uzatma
- Teknik terimleri basit dille açıkla
- Müşterinin adını kullan (varsa)
- Soruları tekrar sorarak doğrulama yap
- Asla '[Şirket Adı]' gibi placeholder metinler kullanma
- Sadece ilk karşılama durumunda 'Merhaba' kullan, her mesajda tekrarlama
- Doğal ve akıcı konuş, gereksiz tekrarlardan kaçın

HATA DURUMLARINDA:
- Özür dile ve anlayış göster
- Alternatif çözümler sun
- Müşteriyi yönlendir (gerekirse)
- Sabırlı ol ve tekrar deneme öner";

            var scenarioPrompt = scenario switch
            {
                "FaturaOdemesi" => @"

FATURA ÖDEMESİ SENARYOSU:
- Fatura ödemesi işlemlerinde yardımcı ol
- Ödeme seçeneklerini açıkla
- Fatura detaylarını kontrol et
- Ödeme onayı ver ve teşekkür et",

                "AbonelikIptali" => @"

ABONELİK İPTALİ SENARYOSU:
- İptal sebebini anlamaya çalış
- Alternatif çözümler öner (indirim, taksit vb.)
- İptal sürecini açıkla
- Müşteriyi ikna etmeye çalış ama zorlama
- İptal kararından emin olduğunu doğrula",

                "FaturaItirazi" => @"

FATURA İTİRAZI SENARYOSU:
- İtiraz sebebini dikkatle dinle
- Anlayış göster ve özür dile
- İtiraz sürecini açıkla
- Gerekli belgeleri iste
- İnceleme süresini belirt",

                "ElektrikArizasi" => @"

ELEKTRİK ARIZASI SENARYOSU:
- Arıza durumunu ciddiye al
- Hemen müdahale edeceğini belirt
- Güvenlik uyarıları ver
- Tahmini süre ver
- Acil durum numaralarını paylaş",

                "Diger" => @"

GENEL İŞLEMLER:
- Genel soruları yanıtla
- Doğru departmana yönlendir
- Bilgi ver ve yardımcı ol
- Müşteri memnuniyetini sağla",

                "SerbestSohbet" => @"

SERBEST SOHBET:
- Doğal ve samimi konuş
- Elektrik konularında da yardım et
- Genel sohbet yap
- Müşteri ile bağ kur",

                "CanliDestek" => @"

CANLI DESTEK:
- Profesyonel ama samimi ol
- Detaylı ve kapsamlı yardım sağla
- Elektrik abonelik konularında uzmanlaş
- Müşteri memnuniyetini ön planda tut
- Gerektiğinde diğer departmanlara yönlendir",

                _ => @"

GENEL ASISTAN:
- Müşteriye yardımcı ol
- Profesyonel davran
- Soruları yanıtla"
            };

            return basePrompt + scenarioPrompt;
        }

        private static string CleanAndOptimizeResponse(string response, string scenario)
        {
            if (string.IsNullOrWhiteSpace(response))
                return "Üzgünüm, şu anda yanıt veremiyorum. Lütfen tekrar deneyiniz.";

            // Yanıtı temizle
            response = response.Trim();
            
            // Mesaj uzunluğu kısıtlaması kaldırıldı - tüm mesaj gösteriliyor

            // Senaryo özel optimizasyonlar (içerik kontrolü)
            switch (scenario)
            {
                case "FaturaOdemesi":
                    if (response.Contains("ödeme") || response.Contains("fatura"))
                        return response;
                    break;
                    
                case "AbonelikIptali":
                    if (response.Contains("iptal") || response.Contains("abonelik"))
                        return response;
                    break;
                    
                case "FaturaItirazi":
                    if (response.Contains("itiraz") || response.Contains("fatura"))
                        return response;
                    break;
                    
                case "ElektrikArizasi":
                    if (response.Contains("arıza") || response.Contains("elektrik"))
                        return response;
                    break;
            }

            return response;
        }
    }
} 