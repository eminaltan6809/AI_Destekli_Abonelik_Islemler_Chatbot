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
        private static string? _apiKey = string.Empty;
        private static string? _model = string.Empty;
        private static bool _initialized = false;

        public static void Initialize(IConfiguration config)
        {
            if (_initialized) return;
            _apiKey = config["OpenAI:ApiKey"];
            _model = config["OpenAI:Model"];
            _initialized = true;
        }

        public static async Task<string> GetReplyAsync(string history, string scenario)
        {
            if (!_initialized || string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_model))
                return "OpenAI API anahtarı veya model ayarı eksik. Lütfen yöneticinize başvurun.";
            if (string.IsNullOrWhiteSpace(history))
                history = "Kullanıcı henüz bir mesaj göndermedi.";
            if (string.IsNullOrWhiteSpace(scenario))
                scenario = "Diger";
            var systemPrompt = scenario == "SerbestSohbet"
                ? "Sen bir sohbet asistanısın. Kullanıcı ile serbestçe sohbet et."
                : $"Sen bir elektrik abonelik işlemleri asistanısın. Senaryo: {scenario}.";
            var requestBody = new
            {
                model = _model,
                messages = new[] {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = history }
                }
            };
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return $"OpenAI API hatası: {response.StatusCode} - {json}";
                dynamic obj = JsonConvert.DeserializeObject(json);
                return obj.choices[0].message.content.ToString();
            }
            catch (Exception ex)
            {
                return $"OpenAI API bağlantı hatası: {ex.Message}";
            }
        }
    }
} 