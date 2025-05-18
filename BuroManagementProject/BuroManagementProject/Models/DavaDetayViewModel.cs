using System;
using System.Collections.Generic;

namespace BuroManagementProject.Models
{

    public class DavaDetayViewModel
    {
        public int DavaID { get; set; }
        public string DavaKonusu { get; set; }
        public int MuvekkilID { get; set; }
        public List<string> AvukatAdlari { get; set; } = new();
        public string? MahkemeAdi { get; set; }
        public string? DavaTaraf { get; set; }
        public DateTime AcilisTarihi { get; set; }
        public DateTime GuncellemeTarihi { get; set; }
        public string? DavaAciklama { get; set; }
        public decimal ToplamUcret { get; set; }
        public decimal OdenenUcret { get; set; }
        public decimal KalanUcret { get; set; }
        public string? DavaAsamasi { get; set; }
        public int Asama_ID { get; set; }

    }

}
