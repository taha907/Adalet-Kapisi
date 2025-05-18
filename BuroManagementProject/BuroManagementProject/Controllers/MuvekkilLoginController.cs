using BuroManagementProject.Data;
using BuroManagementProject.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;

namespace BuroManagementProject.Controllers
{
    public class MuvekkilLoginController : Controller
    {
        

        private readonly KisilerData _data;

        public MuvekkilLoginController(IConfiguration config)
        {
            _data = new KisilerData(config);
        }

        public IActionResult MuvekkilRegister()
        {
            return View(new Kisiler());
        }

        [HttpPost]
        public IActionResult MuvekkilRegister(Kisiler k)
        {
            var sonuc = _data.MuvekkilKayit(k); // string değer dönüyor

            if (sonuc != "Başarılı")
            {
                ViewBag.KayitHatasi = sonuc;
                return View(k); // Kullanıcıyı aynı form ekranına döndür
            }

            return RedirectToAction("login", "Login"); // Kayıt başarılıysa giriş ekranına yönlendir
        }


        public IActionResult Index()
        {
            
            ViewBag.Ad = HttpContext.Session.GetString("KullaniciAd");
            ViewBag.Soyad = HttpContext.Session.GetString("KullaniciSoyad");
            ViewBag.ActiveIndex = "active";
            ViewBag.Title = ""; // Boş bırakarak layout'taki <h1> alanını gizleyebilirsin
            

            return View();
        }
        public IActionResult Profil()
        {
            ViewBag.ActiveProfil = "active";
            ViewBag.Title = "Profil";
          

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
        public IActionResult Avukatlarim()
        {
            ViewBag.Title = "Avukatlarım";
            ViewBag.ActiveAvukatlarim = "active";

            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");

            if (kisiId == null)
                return RedirectToAction("Login", "Login");

            var avukatlarListesi = _data.GetAvukatlarByMuvekkilId(kisiId.Value);

            var model = new AvukatViewModel
            {
                Avukatlar = avukatlarListesi
            };

            return View(model); // artık ViewModel döndürüyoruz
        }

        public IActionResult Davalarim()
        {
            
            ViewBag.Title = "Davalarım";
            ViewBag.ActiveDavalarim = "active";
           

            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");

            if (kisiId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var davalar = _data.GetDavalarByKisiId(kisiId.Value);

            var viewModel = new DavalarViewModel
            {
                Davalar = davalar
            };

            return View(viewModel);
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


        public IActionResult Durusmalarim()
        {
            ViewBag.Title = "Duruşmalarım";
            
            ViewBag.activeDurusmalarim = "active";

            int? kisiId = HttpContext.Session.GetInt32("KullaniciID");

            if (kisiId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var model = new DurusmalarViewModel
            {
                Durusmalar = _data.GetDurusmalarByKisiId(kisiId.Value)
            };
            ViewBag.ActiveDurusmalarim = "active";
            return View(model);
        }

        public IActionResult Hakkimizda()
        {
            ViewBag.ActiveHakkimizda = "active";
            return View();
        }
        public IActionResult Cikis()
        {
            
            return RedirectToAction("login","Login");
        }


    }
}
