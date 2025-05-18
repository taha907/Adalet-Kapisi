using BuroManagementProject.Data;
using BuroManagementProject.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BuroManagementProject.Controllers
{
    public class AvukatLoginController : Controller
    {
        private readonly KisilerData _data;
        public AvukatLoginController(IConfiguration config)
        {
            _data = new KisilerData(config);
        }
        public IActionResult AvukatRegister()
        {
            return View();
        }
        
        
         [HttpPost]
        public IActionResult AvukatRegister(Kisiler k,AdresAvukat a)
        {
            var sonuc = _data.AvukatKayit(k, a); // string değer dönüyor
            if(sonuc != "Başarılı")
            {
                ViewBag.KayitHatasi = sonuc;
                return View(k); // Kullanıcıyı aynı form ekranına döndür
            }
            return RedirectToAction("Login", "Login");
        }

        public IActionResult Index()
        {

            ViewBag.Title = "Avukat Anasayfa";
            ViewBag.ActiveIndex = "active";
            ViewBag.Title = ""; // Boş bırakarak layout'taki <h1> alanını gizleyebilirsin

            return View();
        }
        public IActionResult Profil()
        {
            ViewBag.Title = "Profil";
            ViewBag.ActiveProfil = "active";

            // Session'dan verileri al
            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");
            string? ad = HttpContext.Session.GetString("KullaniciAd");
            string? soyad = HttpContext.Session.GetString("KullaniciSoyad");
            string? tc = HttpContext.Session.GetString("KullaniciTC");
            string? telefon = HttpContext.Session.GetString("KullaniciTelefon");
            string? eposta = HttpContext.Session.GetString("KullaniciEposta");

            // ViewBag veya ViewModel ile View'a gönder
            ViewBag.KisiID = kisiId;
            ViewBag.Ad = ad;
            ViewBag.Soyad = soyad;
            ViewBag.TC = tc;
            ViewBag.Telefon = telefon;
            ViewBag.Eposta = eposta;

            return View();
        }
        public IActionResult Muvekkillerim()
        {
            ViewBag.Title = "Müvekkillerim";
            ViewBag.ActiveAvukatlarim = "active";

            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");
            if (kisiId == null) return RedirectToAction("Login", "Login");

            var muvekkilList = _data.GetMuvekkillerByAvukatId(kisiId.Value);

            var viewModel = new MuvekkilViewModel
            {
                Muvekkiller = muvekkilList
            };

            return View(viewModel);
        }


        public IActionResult DavaEkle()
        {
            var model = new DavaEkleViewModel
            {
                Muvekkiller = _data.GetMuvekkiller()
                    .Select(m => new SelectListItem
                    {
                        Value = m.Kisi_ID.ToString(), // Kisiler tablosundaki anahtar
                        Text = $"{m.Ad} {m.Soyad}"
                    }).ToList(),

                Mahkemeler = _data.GetMahkemeler()
                    .Select(m => new SelectListItem
                    {
                        Value = m.Mahkeme_ID.ToString(),
                        Text = m.Ad
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult DavaEkle(DavaEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Muvekkiller = _data.GetMuvekkiller()
                    .Select(m => new SelectListItem
                    {
                        Value = m.Kisi_ID.ToString(),
                        Text = $"{m.Ad} {m.Soyad}"
                    }).ToList();

                model.Mahkemeler = _data.GetMahkemeler()
                    .Select(m => new SelectListItem
                    {
                        Value = m.Mahkeme_ID.ToString(),
                        Text = m.Ad
                    }).ToList();

                return View(model);
            }

            // 🔑 Avukatın (giriş yapan kullanıcının) ID'sini Session'dan al
            int avukatId = Convert.ToInt32(HttpContext.Session.GetInt32("KullaniciID"));

            // 👇 Avukatı da ekleyecek şekilde fonksiyon güncellendi
            var success = _data.InsertDavaWithMuvekkiller(model.Dava, model.SecilenMuvekkilIdListesi, avukatId);

            if (success)
            {
                TempData["BasariMesaji"] = "Dava başarıyla eklendi.";
                return RedirectToAction("Davalarim");
            }

            ModelState.AddModelError("", "Dava eklenirken bir hata oluştu.");
            return View(model);
        }
        public IActionResult DavaDetayGoster(int id)
        {
            HttpContext.Session.SetInt32("DavaID", id);
            return RedirectToAction("DavaDetay");
        }
        public IActionResult DavaDetay()
        {
            ViewBag.ActiveDavalarim = "active";
            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");
            int? davaId = HttpContext.Session.GetInt32("DavaID");

            if (kisiId == null)
                return RedirectToAction("Login", "Auth");

            if (davaId == null)
                return RedirectToAction("Davalarim", "MuvekkilLogin");

            var model = _data.GetDavaDetayByDavaId(davaId.Value);

            return View(model);
        }



        public IActionResult Davalarim()
        {
            ViewBag.Title = "Avukat Olarak Baktığım Davalar";
            ViewBag.ActiveDavalarim = "active";

            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");
            if (kisiId == null)
                return RedirectToAction("Login", "Login");

            // Yeni fonksiyon ViewModel döndürdüğü için direk alıyoruz
            var viewModel = _data.GetDavalarAvukat2ByKisiId(kisiId.Value);
            return View(viewModel);
        }

        public IActionResult BenimDavalarim()
        {
            ViewBag.Title = "Müvekkil Olarak Bulunduğum Davalar";
            ViewBag.ActiveBenimDavalarim = "active";

            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");
            if (kisiId == null)
                return RedirectToAction("Login", "Login");

            // Yeni fonksiyon ViewModel döndürdüğü için direk alıyoruz
            var viewModel = _data.GetDavalarAvukatByKisiId(kisiId.Value);

            return View(viewModel);
        }

       

        public IActionResult BenimDavaDetayGoster(int id)
        {
            HttpContext.Session.SetInt32("DavaID", id);
            return RedirectToAction("BenimDavaDetay");
        }
        public IActionResult BenimDavaDetay()
        {
            ViewBag.ActiveBenimDavalarim = "active";
            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");
            int? davaId = HttpContext.Session.GetInt32("DavaID");

            if (kisiId == null)
                return RedirectToAction("Login", "Login");

            if (davaId == null)
                return RedirectToAction("Davalarim", "AvukatLogin");

            var model = _data.GetDavaDetayByDavaId(davaId.Value);

            return View(model);
        }


        public IActionResult Durusmalarim()
        {
            ViewBag.Title = "Duruşmalarım";
            ViewBag.ActiveDurusmalarim = "active";

            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");

            if (kisiId == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var model = new DurusmalarViewModel
            {
                Durusmalar = _data.GetAvukatDurusmalarByKisiId(kisiId.Value)
            };
            ViewBag.ActiveDurusmalarim = "active";
            return View(model);
        }

        public IActionResult Hakkimizda()
        {
            ViewBag.Title = "Hakkımızda";
            ViewBag.ActiveHakkimizda = "active";
            return View();
        }
        public IActionResult Cikis()
        {

            return RedirectToAction("login", "Login");
        }


    }

}