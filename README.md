
# Fitness Center Web Project (ASP.NET Core MVC)

Web Programlama dersi proje ödevi kapsamında geliştirilen **Spor Salonu (Fitness Center) Yönetim ve Randevu Sistemi**.

## Proje Özeti
Uygulama; spor salonu hizmetlerini, antrenörleri/uzmanlıklarını, üyelerin randevularını ve yapay zekâ tabanlı egzersiz/diyet önerilerini yönetmeyi hedefler.

## Özellikler

### 1) Spor Salonu ve Hizmet Tanımları
- Spor salonu(ları) oluşturma (tek salon senaryosu da desteklenir)
- Çalışma saatleri tanımlama
- Hizmet türleri (fitness / yoga / pilates vb.)
- Hizmet süre ve ücret bilgileri

### 2) Antrenör (Eğitmen) Yönetimi
- Antrenör ekleme / düzenleme / silme (CRUD)
- Uzmanlık alanları ve verebildiği hizmetlerin tanımlanması
- Müsaitlik saatleri belirleme
- Müsaitlik saatlerine göre randevu alınabilmesi

### 3) Üye & Randevu Sistemi
- Üyelik / kayıt ve giriş
- Uygun antrenör + hizmete göre randevu oluşturma
- Çakışma kontrolü (uygun değilse kullanıcıya uyarı)
- Randevu detaylarının veritabanında saklanması (hizmet, süre, ücret, eğitmen)
- Randevu onay mekanizması

### 4) REST API + LINQ Filtreleme
- En az bir modülde REST API üzerinden veritabanı ile iletişim
- LINQ ile filtreleme (örn: tüm antrenörler, belirli tarihte uygun antrenörler, üyenin randevuları)

### 5) Yapay Zekâ Entegrasyonu
- Fotoğraf yükleme veya boy/kilo/vücut tipi gibi bilgilerle egzersiz/diyet önerisi üretme (OpenAI API veya hazır model)

## Kullanılan Teknolojiler
- ASP.NET Core MVC (LTS) / C#
- Entity Framework Core, LINQ
- SQL Server veya PostgreSQL
- HTML5, CSS3, JavaScript, Bootstrap 5, jQuery

## Roller ve Yetkilendirme
- Rol bazlı yetkilendirme: **Admin** ve **Üye**

## Kurulum ve Çalıştırma

### Gereksinimler
- .NET SDK (uygun sürüm)
- SQL Server veya PostgreSQL
- (Opsiyonel) Visual Studio / VS Code

