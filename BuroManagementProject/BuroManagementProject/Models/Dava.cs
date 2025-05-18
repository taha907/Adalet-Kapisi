
namespace BuroManagementProject.Models
{
    public class Dava
    {
        public int DavaID { get; set; }
        public string? Dava_Konusu { get; set; }
        public string? Acilis_Tarihi { get; set; }
        public string? AvukatAdi { get; set; }
        public string? MuvekkilAdSoyad { get; set; }
        public string? Guncellemetarihi { get; set; }
        public string? Aciklama { get; set; }
        public int? Mahkeme_ID { get; set; }
        public string? DavaDurumu { get; set; } // DavaDurum_ID yerine ismiyle gelsin
        public int? DavaAsama_ID { get; set; }
        public int? DavaTaraf_ID { get; set; }
        public int? ToplamÖdeme { get; set; }
        public int? YapilanÖdeme { get; set; }
      
    }



}