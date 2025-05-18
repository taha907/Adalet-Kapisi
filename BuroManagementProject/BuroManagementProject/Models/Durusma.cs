namespace BuroManagementProject.Models
{
    public class Durusma
    {
        public int DurusmaID { get; set; }
        public string? Tarih { get; set; }
        public string? Saat { get; set; }
        public int DavaID { get; set; }
        public string? DavaKonusu { get; set; }
        public string? MahkemeAdi { get; set; }
        public string? DurusmaDurumu { get; set; }
        public int?  Rol_ID { get; set; }
    }
}
