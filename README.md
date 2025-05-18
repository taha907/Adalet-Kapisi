# Adalet Kapısı - Büro Yönetim Sistemi

## Proje Açıklaması
Adalet Kapısı, avukat bürolarının yönetimini kolaylaştırmak için tasarlanmış bir web tabanlı yönetim sistemidir. Sistem, avukatların müvekkilleri ve davaları hakkında bilgileri düzenli bir şekilde yönetmelerini, duruşma tarihlerini takip etmelerini ve müvekkillerin dava süreçlerini izlemelerini sağlar.

## Teknik Altyapı
- **Platform**: ASP.NET Core MVC (.NET Core)
- **Veritabanı**: MySQL
- **Programlama Dili**: C#
- **Kullanıcı Arayüzü**: HTML, CSS

## Özellikler

### Kullanıcı Yetkilendirmesi
- Farklı kullanıcı rolleri (Avukat, Müvekkil)
- Güvenli giriş sistemi
- Oturum yönetimi

### Avukat Paneli
- Müvekkil yönetimi
- Dava dosyası oluşturma ve takibi
- Duruşma tarihlerini takvimde görüntüleme
- Mahkeme bilgilerini kaydetme ve düzenleme
- Dava detaylarını yönetme

### Müvekkil Paneli
- Davalarını görüntüleme
- Duruşma tarihlerini takip etme
- Avukat ile iletişim

## Kurulum

### Gereksinimler
- .NET Core SDK (en az 6.0)
- MySQL Server
- Visual Studio (önerilen) veya Visual Studio Code

### Veritabanı Kurulumu
1. MySQL Server'ı yükleyin
2. "adalet_kapisi" adında bir veritabanı oluşturun
3. Proje içerisindeki SQL scriptlerini çalıştırarak tabloları oluşturun

### Uygulama Kurulumu
1. Projeyi klonlayın veya indirin
2. `appsettings.json` dosyasındaki veritabanı bağlantı bilgilerini düzenleyin:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "server=localhost;port=3306;user=root;password=root;database=adalet_kapisi;"
   }
   ```
3. Visual Studio'da projeyi açın ve çalıştırın veya terminal üzerinden:
   ```
   cd BuroManagementProject
   dotnet run
   ```

## Kullanım
Uygulama başlatıldığında, kullanıcılar:
1. Giriş sayfasından eposta ve şifreleriyle oturum açabilirler
2. Kullanıcı rolüne göre ilgili panele yönlendirilirler (Avukat/Müvekkil)
3. İlgili işlemleri gerçekleştirebilirler

## Proje Yapısı
- **Controllers/**: MVC mimarisindeki controller sınıfları
- **Models/**: Veri modelleri ve view modelleri
- **Views/**: Kullanıcı arayüzü şablonları
- **Data/**: Veritabanı erişim katmanı
- **wwwroot/**: Statik dosyalar (CSS, JavaScript, resimler)

## Geliştiriciler
[Geliştiricilerin isimleri buraya eklenebilir]

## Lisans
[Lisans bilgileri buraya eklenebilir] 