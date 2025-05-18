using BuroManagementProject.Models;

using MySql.Data.MySqlClient;


namespace BuroManagementProject.Data
{
    public class KisilerData
    {
        private readonly string _connectionString;
        public readonly List<Kisiler>? kisilerList = null;

        public KisilerData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<Kisiler> GetKisiler()
        {
            var kisilerList = new List<Kisiler>(); // BU SATIR EKLENMELİ

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            
            var cmd = new MySqlCommand("SELECT * FROM kisiler", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                
                 kisilerList.Add(new Kisiler
                                {
                Ad = reader["Ad"].ToString(),
                 Soyad = reader["Soyad"].ToString(),
                  Telefon = reader["Telefon"].ToString(),
              Tc = reader["Tc_Kimlik_No"].ToString(),
                Eposta = reader["E_posta"].ToString(),
                       Sifre = reader["Sifre"].ToString(),
                BaroNo = GetNullableString(reader["Baro_No"]),               
                    
                                });
                         
            }

            return kisilerList;
        }

   
        // Avukatın müvekkil olarak yer aldığı davaları getirir ve ViewModel döndürür
        public DavalarViewModel GetDavalarAvukatByKisiId(int kisiId)
        {
            var davalar = new List<Dava>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
    SELECT
        Dava_ID,
        Dava_Konusu,
        GROUP_CONCAT(Davadaki_Avukat SEPARATOR ', ') AS Davadaki_Avukat,
        Acilis_Tarihi,
        Dava_Durumu
    FROM AvukatBenimDavalarim
    WHERE MuvekkilAvukat_ID = @kisiId
    GROUP BY Dava_ID, Dava_Konusu, Acilis_Tarihi, Dava_Durumu
    ORDER BY Acilis_Tarihi DESC;
";


            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kisiId", kisiId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                davalar.Add(new Dava
                {
                    DavaID = reader.GetInt32("Dava_ID"),
                    Dava_Konusu = reader["Dava_Konusu"]?.ToString(),
                    Acilis_Tarihi = reader.GetDateTime("Acilis_Tarihi").ToString("yyyy-MM-dd"),
                    AvukatAdi = reader["Davadaki_Avukat"]?.ToString(),
                    DavaDurumu = reader["Dava_Durumu"]?.ToString()
                });
            }

            return new DavalarViewModel
            {
                Davalar = davalar
            };
        }


        // Avukatın Avukat olarak yer aldığı davaları getirir ve ViewModel döndürür
        public DavalarViewModel GetDavalarAvukat2ByKisiId(int kisiId)
        {
            var davalar = new List<Dava>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
        SELECT 
            Dava_ID,
            Dava_Konusu,
            GROUP_CONCAT(Muvekkil_AdSoyad SEPARATOR ', ') AS Muvekkiller,
            Acilis_Tarihi,
            Dava_Durumu
        FROM avukatindavalarim
        WHERE Avukat_ID = @kisiId
        GROUP BY Dava_ID, Dava_Konusu, Acilis_Tarihi, Dava_Durumu
        ORDER BY Acilis_Tarihi DESC;
    ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kisiId", kisiId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                davalar.Add(new Dava
                {
                    DavaID = reader.GetInt32("Dava_ID"),
                    Dava_Konusu = reader["Dava_Konusu"]?.ToString(),
                    Acilis_Tarihi = reader.GetDateTime("Acilis_Tarihi").ToString("yyyy-MM-dd"),
                    MuvekkilAdSoyad = reader["Muvekkiller"]?.ToString(), // alan adını istersen değiştirirsin
                    DavaDurumu = reader["Dava_Durumu"]?.ToString()
                });
            }

            return new DavalarViewModel
            {
                Davalar = davalar
            };
        }

        public List<Mahkeme> GetMahkemeler()
        {
            var mahkemeler = new List<Mahkeme>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = "SELECT Mahkeme_ID, Ad, Sehir FROM mahkemeler ORDER BY Ad";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                mahkemeler.Add(new Mahkeme
                {
                    Mahkeme_ID = reader.GetInt32("Mahkeme_ID"),
                    Ad = reader.GetString("Ad"),
                    Sehir = reader["Sehir"]?.ToString()
                });
            }

            return mahkemeler;
        }
        public List<Kisiler> GetMuvekkiller()
        {
            var muvekkiller = new List<Kisiler>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
        SELECT k.Kisi_ID, k.Ad, k.Soyad, k.Telefon, k.Tc_Kimlik_No, k.E_posta 
        FROM kisiler k
        JOIN kisi_rol kr ON k.Kisi_ID = kr.Kisi_ID
        WHERE kr.Rol_ID = 2
        ORDER BY k.Ad, k.Soyad";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                muvekkiller.Add(new Kisiler
                {
                    Kisi_ID = Convert.ToInt32(reader["Kisi_ID"]), // Önemli: View'da kullanılacak Value buradan geliyor
                    Ad = reader["Ad"].ToString(),
                    Soyad = reader["Soyad"].ToString(),
                    Telefon = reader["Telefon"].ToString(),
                    Tc = reader["Tc_Kimlik_No"].ToString(),
                    Eposta = reader["E_posta"].ToString()
                });
            }

            return muvekkiller;
        }


        public bool InsertDavaWithMuvekkiller(Dava dava, List<int> muvekkilIds, int avukatId)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string odemeSql = @"
            INSERT INTO odeme (Toplam_Miktar, Odenen_Miktar, Kalan_Miktar)
            VALUES (@toplam, @odenen, @kalan);
            SELECT LAST_INSERT_ID();";

                var kalan = (dava.ToplamÖdeme ?? 0) - (dava.YapilanÖdeme ?? 0);

                using var odemeCmd = new MySqlCommand(odemeSql, conn, transaction);
                odemeCmd.Parameters.AddWithValue("@toplam", dava.ToplamÖdeme ?? 0);
                odemeCmd.Parameters.AddWithValue("@odenen", dava.YapilanÖdeme ?? 0);
                odemeCmd.Parameters.AddWithValue("@kalan", kalan);

                var odemeId = Convert.ToInt32(odemeCmd.ExecuteScalar());

                const string davaSql = @"
            INSERT INTO dava 
            (Dava_Konusu, Acilis_Tarihi, Aciklama, Mahkeme_ID, DavaDurum_ID, Asama_ID, Taraf_ID, Odeme_ID)
            VALUES 
            (@konu, @tarih, @aciklama, @mahkemeId, @durumId, @asamaId, @tarafId, @odemeId);
            SELECT LAST_INSERT_ID();";

                using var davaCmd = new MySqlCommand(davaSql, conn, transaction);
                davaCmd.Parameters.AddWithValue("@konu", dava.Dava_Konusu);
                davaCmd.Parameters.AddWithValue("@tarih", DateTime.Now.ToString("yyyy-MM-dd"));
                davaCmd.Parameters.AddWithValue("@aciklama", dava.Aciklama ?? "");
                davaCmd.Parameters.AddWithValue("@mahkemeId", dava.Mahkeme_ID ?? (object)DBNull.Value);
                davaCmd.Parameters.AddWithValue("@durumId", 1); // Başlangıç durumu
                davaCmd.Parameters.AddWithValue("@asamaId", dava.DavaAsama_ID ?? (object)DBNull.Value);
                davaCmd.Parameters.AddWithValue("@tarafId", dava.DavaTaraf_ID ?? (object)DBNull.Value);
                davaCmd.Parameters.AddWithValue("@odemeId", odemeId);

                var davaId = Convert.ToInt32(davaCmd.ExecuteScalar());

                // Müvekkilleri dava_taraf'a ekle
                foreach (var muvekkilId in muvekkilIds)
                {
                    const string davaTarafSql = @"
                INSERT INTO dava_taraf (Dava_ID, Kisi_ID, Rol_ID)
                VALUES (@davaId, @kisiId, @rolId);";

                    using var muvekkilCmd = new MySqlCommand(davaTarafSql, conn, transaction);
                    muvekkilCmd.Parameters.AddWithValue("@davaId", davaId);
                    muvekkilCmd.Parameters.AddWithValue("@kisiId", muvekkilId);
                    muvekkilCmd.Parameters.AddWithValue("@rolId", 2); // Müvekkil

                    muvekkilCmd.ExecuteNonQuery();
                }

                // Avukatı dava_taraf'a ekle
                const string avukatTarafSql = @"
            INSERT INTO dava_taraf (Dava_ID, Kisi_ID, Rol_ID)
            VALUES (@davaId, @kisiId, @rolId);";

                using var avukatCmd = new MySqlCommand(avukatTarafSql, conn, transaction);
                avukatCmd.Parameters.AddWithValue("@davaId", davaId);
                avukatCmd.Parameters.AddWithValue("@kisiId", avukatId);
                avukatCmd.Parameters.AddWithValue("@rolId", 1); // Avukat

                avukatCmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }



        public List<Dava> GetDavalarByKisiId(int kisiId)
        {
            var davalar = new List<Dava>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = @"
SELECT 
    d.Dava_ID,
    d.Dava_Konusu,
    d.Acilis_Tarihi,
    GROUP_CONCAT(CONCAT(k_avukat.Ad, ' ', k_avukat.Soyad) SEPARATOR ', ') AS AvukatAdi,
    dd.Dava_Turu AS DavaDurumu
FROM dava d

-- Müvekkil filtrelemesi
JOIN dava_taraf dt_muvekkil 
    ON d.Dava_ID = dt_muvekkil.Dava_ID 
    AND dt_muvekkil.Kisi_ID = @kisiId 
    AND dt_muvekkil.Rol_ID = 2

-- Avukatlar
JOIN dava_taraf dt_avukat 
    ON d.Dava_ID = dt_avukat.Dava_ID 
    AND dt_avukat.Rol_ID = 1

JOIN kisiler k_avukat 
    ON dt_avukat.Kisi_ID = k_avukat.Kisi_ID

LEFT JOIN dava_durum dd 
    ON d.DavaDurum_ID = dd.DavaDurum_ID

GROUP BY d.Dava_ID, d.Dava_Konusu, d.Acilis_Tarihi, dd.Dava_Turu

";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kisiId", kisiId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                davalar.Add(new Dava
                {
                    DavaID = Convert.ToInt32(reader["Dava_ID"]),
                    Dava_Konusu = reader["Dava_Konusu"].ToString(),
                    Acilis_Tarihi = Convert.ToDateTime(reader["Acilis_Tarihi"]).ToString("yyyy-MM-dd"),
                    AvukatAdi = reader["AvukatAdi"]?.ToString(),
                    DavaDurumu = reader["DavaDurumu"]?.ToString()
                });
            }

            return davalar;
        }
        public List<Avukat> GetAvukatlarByMuvekkilId(int muvekkilId)
        {
            var avukatlar = new List<Avukat>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = @"
    SELECT 
        Avukat_AdSoyad,
        Telefon,
        Avukat_Adres
    FROM MuvekkilinAvukatlari
    WHERE Muvekkil_ID = @muvekkilId";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@muvekkilId", muvekkilId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var adSoyad = reader["Avukat_AdSoyad"]?.ToString() ?? "";
                var isimParcala = adSoyad.Split(' ', 2); // Ad ve Soyad'ı ayır

                var adresStr = reader["Avukat_Adres"]?.ToString() ?? "";
                var adresParcala = adresStr.Split(',', 3);

                var adres = new AdresAvukat
                {
                    Il = adresParcala.Length > 0 ? adresParcala[0].Trim() : "",
                    Ilce = adresParcala.Length > 1 ? adresParcala[1].Trim() : "",
                    Adres = adresParcala.Length > 2 ? adresParcala[2].Trim() : ""
                };

                var kisi = new Kisiler
                {
                    Ad = isimParcala.Length > 0 ? isimParcala[0] : "",
                    Soyad = isimParcala.Length > 1 ? isimParcala[1] : "",
                    Telefon = reader["Telefon"]?.ToString() ?? ""
                };

                var avukat = new Avukat
                {
                    Kisiler = kisi,
                    Adres = adres
                };

                avukatlar.Add(avukat);
            }

            return avukatlar;
        }
        public List<Muvekkil> GetMuvekkillerByAvukatId(int avukatId)
        {
            var muvekkiller = new List<Muvekkil>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
        SELECT 
            Muvekkil_AdSoyad,
            Telefon,
            E_posta,
            Aktif_Dava_Sayisi
        FROM AvukatinMuvekkilleri
        WHERE Avukat_ID = @avukatId;
    ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@avukatId", avukatId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                muvekkiller.Add(new Muvekkil
                {
                    AdSoyad = reader["Muvekkil_AdSoyad"]?.ToString() ?? "",
                    Telefon = reader["Telefon"]?.ToString() ?? "",
                    Eposta = reader["E_posta"]?.ToString() ?? "",
                    AktifDavaSayisi = Convert.ToInt32(reader["Aktif_Dava_Sayisi"])
                });
            }

            return muvekkiller;
        }


        public DavaDetayViewModel GetDavaDetayByDavaId(int davaId)
        {
            var detay = new DavaDetayViewModel();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT * FROM muvekkildavadetay WHERE Dava_ID = @davaId";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@davaId", davaId);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                detay.DavaID = Convert.ToInt32(reader["Dava_ID"]);
                detay.DavaKonusu = reader["Dava_Konusu"]?.ToString();
                detay.MuvekkilID = Convert.ToInt32(reader["Muvekkil_ID"]);

                var avukatAdlari = reader["Avukat_AdSoyad"]?.ToString();
                if (!string.IsNullOrWhiteSpace(avukatAdlari))
                    detay.AvukatAdlari = avukatAdlari.Split(',').Select(s => s.Trim()).ToList();

                detay.MahkemeAdi = reader["Mahkeme_Adi"]?.ToString();
                detay.DavaTaraf = reader["Dava_Taraf"]?.ToString();
                detay.AcilisTarihi = Convert.ToDateTime(reader["Acilis_Tarihi"]);
                detay.GuncellemeTarihi = Convert.ToDateTime(reader["Guncelleme_Tarihi"]);
                detay.DavaAciklama = reader["Dava_Aciklama"]?.ToString();
                detay.ToplamUcret = reader["Toplam_Miktar"] != DBNull.Value ? Convert.ToDecimal(reader["Toplam_Miktar"]) : 0;
                detay.OdenenUcret = reader["Odenen_Miktar"] != DBNull.Value ? Convert.ToDecimal(reader["Odenen_Miktar"]) : 0;
                detay.KalanUcret = reader["Kalan_Miktar"] != DBNull.Value ? Convert.ToDecimal(reader["Kalan_Miktar"]) : 0;
                detay.DavaAsamasi = reader["Dava_Asamasi"]?.ToString();
                detay.Asama_ID = reader["Asama_ID"] != DBNull.Value ? Convert.ToInt32(reader["Asama_ID"]) : 0;
            }

            return detay;
        }

        //view kullanarak benimdava detay 
  

        public List<Durusma> GetDurusmalarByKisiId(int kisiId)
        {
            var durusmalar = new List<Durusma>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = @"
SELECT 
    du.Durusma_ID,
    du.Tarih,
    du.Saat,
    d.Dava_ID,
    d.Dava_Konusu,
    m.Ad,
    dd.Durusma_Turu AS DurusmaDurumu
FROM durusmalar du
JOIN dava d ON du.Dava_ID = d.Dava_ID
JOIN mahkemeler m ON du.Mahkeme_ID = m.Mahkeme_ID
JOIN durusma_durum dd ON du.DurusmaDurum_ID = dd.DurusmaDurum_ID
JOIN dava_taraf dt ON d.Dava_ID = dt.Dava_ID
WHERE dt.Kisi_ID = @kisiId;
";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kisiId", kisiId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                durusmalar.Add(new Durusma
                {
                    DurusmaID = Convert.ToInt32(reader["Durusma_ID"]),
                    Tarih = Convert.ToDateTime(reader["Tarih"]).ToString("dd.MM.yyyy"),
                    Saat = reader["Saat"].ToString(), // hh:mm
                    DavaID = Convert.ToInt32(reader["Dava_ID"]),
                    DavaKonusu = reader["Dava_Konusu"].ToString(),
                    MahkemeAdi = reader["Ad"].ToString(),
                    DurusmaDurumu = reader["DurusmaDurumu"].ToString(),
                   
                });
            }

            return durusmalar;
        }
        public List<Durusma> GetAvukatDurusmalarByKisiId(int kisiId)
        {
            var durusmalar = new List<Durusma>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var sql = @"
        SELECT 
            Dava_ID,
            Dava_Konusu,
            Durusma_Tarihi,
            Durusma_Saati,
            Mahkeme_Adi,
            Durusma_Durumu,
            Rol_ID
        FROM AvukatDurusmaBilgileri
        WHERE Katilimci_ID = @kisiId;
    ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kisiId", kisiId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                durusmalar.Add(new Durusma
                {
                    DavaID = Convert.ToInt32(reader["Dava_ID"]),
                    DavaKonusu = reader["Dava_Konusu"].ToString(),
                    Tarih = Convert.ToDateTime(reader["Durusma_Tarihi"]).ToString("dd.MM.yyyy"),
                    Saat = reader["Durusma_Saati"].ToString(),
                    MahkemeAdi = reader["Mahkeme_Adi"].ToString(),
                    DurusmaDurumu = reader["Durusma_Durumu"].ToString(),
                    Rol_ID = Convert.ToInt32(reader["Rol_ID"])
                });
            }

            return durusmalar;
        }

        public string MuvekkilKayit(Kisiler k)
        {
             
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                var tcKontrolCmd = new MySqlCommand("SELECT COUNT(*) FROM kisiler WHERE Tc_Kimlik_No = @TC", conn, transaction);
                tcKontrolCmd.Parameters.AddWithValue("@TC", k.Tc);
                if (Convert.ToInt32(tcKontrolCmd.ExecuteScalar()) > 0)
                    return "Bu TC kimlik numarası zaten kayıtlı!";

                var mailKontrolCmd = new MySqlCommand("SELECT COUNT(*) FROM kisiler WHERE E_posta = @Eposta", conn, transaction);
                mailKontrolCmd.Parameters.AddWithValue("@Eposta", k.Eposta);
                if (Convert.ToInt32(mailKontrolCmd.ExecuteScalar()) > 0)
                    return "Bu e-posta adresi zaten kayıtlı!";

                var telefonKontrolCmd = new MySqlCommand("SELECT COUNT(*) FROM kisiler WHERE Telefon = @Telefon", conn, transaction);
                telefonKontrolCmd.Parameters.AddWithValue("@Telefon", k.Telefon);
                if (Convert.ToInt32(telefonKontrolCmd.ExecuteScalar()) > 0)
                    return "Bu telefon numarası zaten kayıtlı!";

                // Kayıt
                var kisiEkleCmd = new MySqlCommand(@"
            INSERT INTO kisiler (Ad, Soyad, Telefon, Tc_Kimlik_No, E_posta, Sifre) 
            VALUES (@Ad, @Soyad, @Telefon, @TC, @Eposta, @Sifre); 
            SELECT LAST_INSERT_ID();", conn, transaction);

                kisiEkleCmd.Parameters.AddWithValue("@Ad", k.Ad);
                kisiEkleCmd.Parameters.AddWithValue("@Soyad", k.Soyad);
                kisiEkleCmd.Parameters.AddWithValue("@Telefon", k.Telefon);
                kisiEkleCmd.Parameters.AddWithValue("@TC", k.Tc);
                kisiEkleCmd.Parameters.AddWithValue("@Eposta", k.Eposta);
                kisiEkleCmd.Parameters.AddWithValue("@Sifre", k.Sifre);

                var Kisi_ID = Convert.ToInt32(kisiEkleCmd.ExecuteScalar());

                var rolEkleCmd = new MySqlCommand("INSERT INTO kisi_rol (Kisi_ID, Rol_ID) VALUES (@Kisi_ID, @Otomatik_Rol_ID)", conn, transaction);
                rolEkleCmd.Parameters.AddWithValue("@Kisi_ID", Kisi_ID);
                rolEkleCmd.Parameters.AddWithValue("@Otomatik_Rol_ID", 2);
                rolEkleCmd.ExecuteNonQuery();

                transaction.Commit();
                return "Başarılı";
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return "Kayıt işlemi başarısız: " + e.Message;
            }
        }

        public string AvukatKayit(Kisiler k, AdresAvukat a)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // TC kontrolü
                var tcKontrolCmd = new MySqlCommand("SELECT COUNT(*) FROM kisiler WHERE Tc_Kimlik_No = @TC", conn, transaction);
                tcKontrolCmd.Parameters.AddWithValue("@TC", k.Tc);
                if (Convert.ToInt32(tcKontrolCmd.ExecuteScalar()) > 0)
                    return "Bu TC kimlik numarası zaten kayıtlı!";

                // E-posta kontrolü
                var mailKontrolCmd = new MySqlCommand("SELECT COUNT(*) FROM kisiler WHERE E_posta = @Eposta", conn, transaction);
                mailKontrolCmd.Parameters.AddWithValue("@Eposta", k.Eposta);
                if (Convert.ToInt32(mailKontrolCmd.ExecuteScalar()) > 0)
                    return "Bu e-posta adresi zaten kayıtlı!";

                // Telefon kontrolü
                var telefonKontrolCmd = new MySqlCommand("SELECT COUNT(*) FROM kisiler WHERE Telefon = @Telefon", conn, transaction);
                telefonKontrolCmd.Parameters.AddWithValue("@Telefon", k.Telefon);
                if (Convert.ToInt32(telefonKontrolCmd.ExecuteScalar()) > 0)
                    return "Bu telefon numarası zaten kayıtlı!";

                // Adres tablosuna ekleme
                var adresCmd = new MySqlCommand(@"
            INSERT INTO adres_avukat (Il, Ilce, Adres) 
            VALUES (@Il, @Ilce, @Adres); 
            SELECT LAST_INSERT_ID();", conn, transaction);

                adresCmd.Parameters.AddWithValue("@Il", a.Il);
                adresCmd.Parameters.AddWithValue("@Ilce", a.Ilce);
                adresCmd.Parameters.AddWithValue("@Adres", a.Adres);

                var adresID = Convert.ToInt32(adresCmd.ExecuteScalar());

                // Kisiler tablosuna ekleme
                var kisiEkleCmd = new MySqlCommand(@"
            INSERT INTO kisiler 
            (Ad, Soyad, Telefon, Tc_Kimlik_No, E_posta, Sifre, Baro_No, Adres_ID) 
            VALUES (@Ad, @Soyad, @Telefon, @TC, @Eposta, @Sifre, @BaroNo, @AdresID);
            SELECT LAST_INSERT_ID();", conn, transaction);

                kisiEkleCmd.Parameters.AddWithValue("@Ad", k.Ad);
                kisiEkleCmd.Parameters.AddWithValue("@Soyad", k.Soyad);
                kisiEkleCmd.Parameters.AddWithValue("@Telefon", k.Telefon);
                kisiEkleCmd.Parameters.AddWithValue("@TC", k.Tc);
                kisiEkleCmd.Parameters.AddWithValue("@Eposta", k.Eposta);
                kisiEkleCmd.Parameters.AddWithValue("@Sifre", k.Sifre);
                kisiEkleCmd.Parameters.AddWithValue("@BaroNo", k.BaroNo);
                kisiEkleCmd.Parameters.AddWithValue("@AdresID", adresID);

                var kisiID = Convert.ToInt32(kisiEkleCmd.ExecuteScalar());

                // kisi_rol tablosuna ekleme
                var rolEkleCmd = new MySqlCommand("INSERT INTO kisi_rol (Kisi_ID, Rol_ID) VALUES (@KisiID, @Otomatik_Rol_Id)", conn, transaction);
                rolEkleCmd.Parameters.AddWithValue("@KisiID", kisiID);
                rolEkleCmd.Parameters.AddWithValue("@Otomatik_Rol_ID", 1); // 1 = Avukat
                rolEkleCmd.ExecuteNonQuery();

                transaction.Commit();
                return "Başarılı";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return "Kayıt işlemi başarısız: " + ex.Message;
            }
        }

        public int? GetRol_ID_By_Mail_Password(string mail, string password)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
        SELECT Rol_ID FROM kisi_rol 
        WHERE Kisi_ID = (
            SELECT Kisi_ID FROM kisiler 
            WHERE E_posta = @mail AND Sifre = @password
        );", conn);

            cmd.Parameters.AddWithValue("@mail", mail);
            cmd.Parameters.AddWithValue("@password", password);

            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : (int?)null;
        }

        public int GetKisi_ID_By_Mail_Password(string mail, string password)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var getKisiCmd = new MySqlCommand(
                "SELECT Kisi_ID FROM kisiler WHERE E_posta = @mail AND Sifre = @password;", conn);

            getKisiCmd.Parameters.AddWithValue("@mail", mail);
            getKisiCmd.Parameters.AddWithValue("@password", password);

            var result = getKisiCmd.ExecuteScalar();

            return result != null ? Convert.ToInt32(result) : -1;
        }

        public static string GetNullableString(object obj)
        {
            return obj != DBNull.Value ? obj.ToString() : null;
        }

        
       
        
    }
}

