using System;
namespace AI_Destekli_Abonelik_Chatbot.Models
{
    public class ChatHistory
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Scenario { get; set; }
        public string? History { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 