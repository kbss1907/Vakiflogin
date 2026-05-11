# Vakiflogin Kurumsal Yönetim Sistemi 🚀

Vakiflogin, modern web teknolojileri kullanılarak geliştirilmiş, yüksek güvenlik standartlarına sahip kurumsal bir çalışan ve izin yönetimi platformudur.

## 🌟 Özellikler

- **Gelişmiş Güvenlik:** Asimetrik şifreleme (RSA) ve stateful JWT tabanlı kimlik doğrulama.
- **Çalışan Yönetimi:** Rol bazlı yetkilendirme (Admin, İK Yöneticisi, Yönetici, Çalışan).
- **İzin Yönetimi (Leave Management):** Hiyerarşik izin onay mekanizması.
- **Kullanıcı Dostu Arayüz:** Angular ve Tailwind CSS ile geliştirilmiş modern, hızlı ve duyarlı (responsive) tasarım.
- **Güvenli Şifre Sıfırlama:** Token tabanlı e-posta şifre sıfırlama altyapısı.

## 🛠️ Kullanılan Teknolojiler

### Backend
- **Framework:** .NET 8 / 9 (ASP.NET Core Web API)
- **Veritabanı:** Microsoft SQL Server (Stored Procedure tabanlı mimari)
- **Güvenlik:** JWT Bearer, RSA Şifreleme (OAEP SHA-256), Anti-Forgery (XSRF), Rate Limiting
- **Önbellekleme:** IMemoryCache (Stateful Session Management)

### Frontend
- **Framework:** Angular 17/18
- **Stil & UI:** Tailwind CSS
- **Şifreleme:** Web Crypto API

## 🚀 Kurulum ve Çalıştırma

### 1. Gereksinimler
- Node.js (v18+)
- .NET SDK (v8.0+)
- SQL Server (LocalDB veya standart sürüm)

### 2. Backend Kurulumu
1. `VAKİFLOGİN` (API) klasörüne gidin.
2. Geliştirme ortamı için yerel veritabanı ayarlarınızı ve gizli anahtarlarınızı oluşturun (Güvenlik gereği `.gitignore` ile repodan çıkarılmıştır):
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=AuthSpDb;Trusted_Connection=True;"
dotnet user-secrets set "Jwt:Key" "GIZLI_JWT_ANAHTARINIZ"
```
3. Projeyi derleyip çalıştırın:
```bash
dotnet run
```

### 3. Frontend Kurulumu
1. `vakif-login-ui` klasörüne gidin.
2. Gerekli paketleri yükleyin:
```bash
npm install
```
3. Geliştirme sunucusunu başlatın:
```bash
npm start
```

## 🔒 Güvenlik Notları
- Bu repository, hassas veriler (API key'ler, RSA private key, Connection String'ler vb.) temizlenerek açık kaynak standartlarına uygun hale getirilmiştir.
- Gerçek ortamda (`Production`) şifrelerin ve gizli anahtarların güvenli bir Environment (Çevre Değişkeni) mekanizması üzerinden verilmesi gerekmektedir.

## 📄 Lisans
Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.
