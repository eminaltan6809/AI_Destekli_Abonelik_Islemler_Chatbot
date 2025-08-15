namespace AI_Destekli_Abonelik_Chatbot.Models
{
    public class Abone
    {
        // Mevcut alanlar
        public string IsimSoyisim { get; set; } = string.Empty;
        public long TcKimlikNo { get; set; }
        public int AboneNo { get; set; }
        public long Telefon { get; set; }
        public int FaturaId { get; set; }
        public string Adres { get; set; } = string.Empty;
        public int OdenecekFatura { get; set; } // 3 basamaklı fatura tutarı
        
        // Abonelik iptali senaryoları için gerekli yeni alanlar
        public string BabaAdi { get; set; } = string.Empty;
        public DateTime DogumTarihi { get; set; }
        public string SozlesmeHesapNo { get; set; } = string.Empty; // 106 ile başlayan 10 haneli
        public string IbanNumarasi { get; set; } = string.Empty;
        public decimal GuvenceBedeli { get; set; }
        public decimal GuncelBorc { get; set; }
        public string AbonelikDurumu { get; set; } = "Normal"; // Normal, Kesik, Askıda vb.
        public bool TuzelAbonelik { get; set; } = false;
        public string TuzelKurumAdi { get; set; } = string.Empty;
        public string YetkiliKisiAdi { get; set; } = string.Empty;
        public bool VefatDurumu { get; set; } = false;
        public string VarisBilgisi { get; set; } = string.Empty; // Tek varis, Çoklu varis
        public string SayacSeriNo { get; set; } = string.Empty;
        public DateTime AbonelikBaslangicTarihi { get; set; }
        public DateTime? AbonelikBitisTarihi { get; set; }
        public bool AktifAbonelik { get; set; } = true;
        public string AbonelikTipi { get; set; } = "Konut"; // Konut, Ticari, Şantiye vb.
        public string OzelDurumNotu { get; set; } = string.Empty; // Şantiye, vefat vb. özel durumlar için
        public DateTime? SonIslemTarihi { get; set; }
        public string IslemDurumu { get; set; } = "Beklemede"; // Beklemede, Tamamlandı, İptal Edildi
        public bool CanliDestekAktarim { get; set; } = false;
    }
} 