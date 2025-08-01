namespace AI_Destekli_Abonelik_Chatbot.Models
{
    public class ChatSessionData
    {
        // Mevcut alanlar
        public string? IsimSoyisim { get; set; } = string.Empty;
        public string? TcKimlikNo { get; set; } = string.Empty;
        public string? AboneNo { get; set; } = string.Empty;
        public string? FaturaId { get; set; } = string.Empty;
        public string? ItirazSebebi { get; set; } = string.Empty;
        public string? Adres { get; set; } = string.Empty;
        public string? ArizaAciklamasi { get; set; } = string.Empty;
        
        // Abonelik iptali senaryoları için yeni alanlar
        public string? AboneSahibiMi { get; set; } = string.Empty; // Evet/Hayır/Vefat/Şirket
        public string? BabaAdi { get; set; } = string.Empty;
        public string? DogumTarihi { get; set; } = string.Empty; // GG/AA/YYYY formatında
        public string? SozlesmeHesapNo { get; set; } = string.Empty; // 106 ile başlayan 10 haneli
        public string? IbanNumarasi { get; set; } = string.Empty;
        public string? EnerjiKesmeTarihi { get; set; } = string.Empty;
        public string? BorcKontrolu { get; set; } = string.Empty; // Yapıldı/Yapılmadı
        public string? AbonelikDurumu { get; set; } = string.Empty; // Normal/Kesik/Askıda
        public string? VefatDurumu { get; set; } = string.Empty; // Evet/Hayır
        public string? VarisBilgisi { get; set; } = string.Empty; // Tek varis/Çoklu varis
        public string? TuzelAbonelik { get; set; } = string.Empty; // Evet/Hayır
        public string? SistemselHata { get; set; } = string.Empty; // Evet/Hayır
        public string? CanliDestekAktarim { get; set; } = string.Empty; // Evet/Hayır
        public string? IslemAdimi { get; set; } = string.Empty; // Hangi adımda olduğumuzu takip etmek için
    }
} 