using System;

namespace AI_Destekli_Abonelik_Chatbot.Models
{
    public class ChatHistory
    {
        public string SessionId { get; set; } = string.Empty;
        public string Messages { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 