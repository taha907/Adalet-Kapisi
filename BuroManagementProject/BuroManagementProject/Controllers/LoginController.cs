using BuroManagementProject.Data;
using BuroManagementProject.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace BuroManagementProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly KisilerData _data;

        public LoginController(IConfiguration config)
        {
            _data = new KisilerData(config);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Hata = "Lütfen tüm alanları doldurunuz.";
                return View();
            }


            var kisilerList = _data.GetKisiler();

            if (kisilerList == null)
            {
                ViewBag.Hata = "Kullanıcı listesi alınamadı.";
                return View();
            }

            var kullanıcı = kisilerList.FirstOrDefault(k =>
                k.Eposta == email && k.Sifre == password);
            

            if (kullanıcı != null)
            {
                int? controlRolIdValue = _data.GetRol_ID_By_Mail_Password(email, password);
                int KisiID = _data.GetKisi_ID_By_Mail_Password(email, password);

                if (controlRolIdValue == null)
                {
                    ViewBag.Hata = "Geçersiz e-posta veya şifre. control null";
                    return View();
                }
                else
                {
                    HttpContext.Session.SetInt32("KullaniciID", KisiID);
                    HttpContext.Session.SetString("KullaniciAd", kullanıcı.Ad);
                    HttpContext.Session.SetString("KullaniciSoyad", kullanıcı.Soyad);
                    HttpContext.Session.SetString("KullaniciTC", kullanıcı.Tc);
                    HttpContext.Session.SetString("KullaniciTelefon", kullanıcı.Telefon);
                    HttpContext.Session.SetString("KullaniciEposta", kullanıcı.Eposta);
                       if (controlRolIdValue == 1)
                    {

                        return RedirectToAction("Index", "AvukatLogin");
                    }
                    else if(controlRolIdValue == 2)
                    {
                        return RedirectToAction("Index", "MuvekkilLogin");
                    }
                }
                         }
            else
            {
                ViewBag.Hata = "Geçersiz e-posta veya şifre. kullanıcı null";

               return View();
            }

            return View();
        }

    }
}
