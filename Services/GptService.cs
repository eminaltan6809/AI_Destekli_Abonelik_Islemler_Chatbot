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

        public static async Task<string> GetReplyAsync(string history, string scenario)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Google AI Studio API anahtarı eksik. Lütfen yöneticinize başvurun.";
            if (string.IsNullOrWhiteSpace(history))
                history = "Kullanıcı henüz bir mesaj göndermedi.";
            if (string.IsNullOrWhiteSpace(scenario))
                scenario = "Diger";
            var systemPrompt = scenario == "SerbestSohbet"
                ? "Sen bir sohbet asistanısın. Kullanıcı ile serbestçe sohbet et."
                : $"Sen bir elektrik abonelik işlemleri asistanısın. Senaryo: {scenario}.";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[] {
                            new { text = systemPrompt + "\n" + history }
                        }
                    }
                }
            };
            try
            {
                using var client = new HttpClient();
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_endpoint + _apiKey, content);
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return $"Google AI Studio API hatası: {response.StatusCode} - {json}";
                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                // Gemini Pro cevabı: obj.candidates[0].content.parts[0].text
                return obj.candidates[0].content.parts[0].text.ToString();
            }
            catch (Exception ex)
            {
                return $"Google AI Studio API bağlantı hatası: {ex.Message}";
            }
        }
    }
} 