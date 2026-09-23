# Memetoğlu Taşımacılık — Web Sitesi (ASP.NET Core)

Yönetim paneli olan kurumsal web sitesi. **Razor Pages**, .NET 10 (LTS).

- **Hiçbir NuGet paketi kullanmaz** — yalnızca .NET'in kendi çerçevesi. Paket sürümü kaynaklı sorun çıkmaz.
- **Veritabanı gerekmez** — tüm içerik `App_Data/content.json` dosyasında durur. Yedek almak bu dosyayı kopyalamak demektir.
- Plesk/IIS üzerinde çalışır; sunucu kiralamaya gerek yoktur.

---

## 1. Localhost'ta çalıştırma (2 dakika)

Makinenizde .NET SDK zaten kurulu. Proje klasöründe PowerShell açın
(klasörde **Shift + sağ tık** → *PowerShell penceresini burada aç*):

```powershell
dotnet run
```

İlk çalıştırma birkaç saniye sürer. Ardından:

| Adres | Ne var |
| --- | --- |
| <http://localhost:5080> | Site |
| <http://localhost:5080/Panel/> | Yönetim paneli (ilk açılışta kurulum ekranı) |

Visual Studio ile çalışmak isterseniz `MemetogluWeb.csproj` dosyasına çift tıklayıp **F5**'e basmanız yeterli.

### "Uygulama Denetimi ilkesi bu dosyayı engelledi" hatası

Bu makinede kurumsal bir **Windows Uygulama Denetimi (WDAC/AppLocker)** ilkesi var ve
imzasız `.exe` dosyalarını engelliyor. Bu yüzden proje artık hiç `.exe` üretmiyor
(`MemetogluWeb.csproj` içinde `UseAppHost=false`): F5 ve `dotnet run` uygulamayı
Microsoft imzalı `dotnet.exe` ile başlatır, engele takılmaz.

### Tek tıkla güncelle ve çalıştır: `calistir.bat`

Proje klasöründeki **`calistir.bat`** dosyasına çift tıklayın. Sırasıyla:

1. GitHub'daki son değişiklikleri indirir (`git pull`),
2. 5080 portunda çalışan eski sürümü kapatır,
3. Siteyi başlatır ve tarayıcıda `http://localhost:5080` adresini açar.

Güncellemenin çalışması için klasörün ZIP'ten değil `git clone` ile indirilmiş olması
gerekir. Git kurulu değilse betik Visual Studio'nun kendi Git'ini kullanmayı dener.

> Bu kısıt yalnızca kendi bilgisayarınızda geçerlidir. Plesk/IIS sunucusunda böyle bir
> ilke olmadığı için yayın ortamında hiç karşınıza çıkmaz.

*Durum: proje bu makinede **0 hata, 0 uyarı** ile derlendi ve
`http://localhost:5080` üzerinde sorunsuz çalıştı.*

### İlk kurulum

`/Panel/` adresini ilk açtığınızda kurulum ekranı gelir:

1. Yönetici e-postası ve şifresi (en az 10 karakter)
2. Teklif formunun düşeceği e-posta adresi
3. SMTP bilgileri — şimdilik boş bırakabilirsiniz, sonra eklersiniz

Kaydedince `App_Data/config.json` oluşur ve bu ekran bir daha açılmaz.

> **Şifrenizi unutursanız:** `App_Data/config.json` dosyasını silin; kurulum ekranı yeniden açılır.

---

## 2. Paneli kullanma

| Bölüm | Ne düzenlenir |
| --- | --- |
| **Anasayfa** | Her bölümün başlığı, metni, görseli; hero videosu; rakamlar |
| **Hizmetler** | Hizmet kartları — ekleme, düzenleme, silme |
| **Filo** | Araç tipleri ve özellikleri |
| **Nasıl Çalışır** | Süreç adımları |
| **Site Ayarları** | Telefon, e-posta, adres, WhatsApp, sosyal medya, SEO |
| **Görseller** | Yüklenen fotoğrafların listesi |
| **Şifre** | Şifre değiştirme ve e-posta ayarları |

Çalışma mantığı:

- Her sayfanın altındaki **Kaydet** o sayfadaki her şeyi birlikte kaydeder
- Kaydetmeden çıkmaya kalkarsanız tarayıcı uyarır
- Her kayıtta `App_Data/yedek/` klasörüne otomatik yedek alınır (son 20 sürüm)
- Bir şey ters giderse yedek dosyasını `App_Data/content.json` üzerine kopyalayın

### Yayına geçerken kapatılacak iki uyarı

1. **Anasayfa → Rakamlar →** "Örnek veriler uyarısı gösterilsin" işaretini kaldırın
2. **Site Ayarları → Taslak uyarısı →** işareti kaldırın

---

## 3. Hosting seçimi

Payload/Node gerektirmez; **ASP.NET Core destekleyen bir Windows (Plesk) paketi** yeterlidir.

Hosting alırken sağlayıcıya sorulacak tek soru:

> "Paketinizde ASP.NET Core **.NET 10** destekleniyor mu?"

Cevap hayırsa iki seçeneğiniz var:

**a) Hedefi düşürün.** `MemetogluWeb.csproj` içinde tek satır:

```xml
<TargetFramework>net8.0</TargetFramework>
```

**b) Kendi kendine yeten yayın yapın.** Sunucuda .NET kurulu olmasına gerek kalmaz:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -o .\publish
```

Bu yöntemde çıktı ~70 MB olur ama sunucunun .NET sürümü hiç önemli olmaz.

Türkiye'de Plesk + ASP.NET Core veren sağlayıcılar: Natro, Turhost, Guzel.net, Isimtescil.
Yıllık maliyet genellikle 1.000–2.500 TL bandındadır.

---

## 4. Yayına alma (Plesk)

### 4.1 Yayın paketi oluşturun

```powershell
dotnet publish -c Release -o .\publish
```

`publish` klasörünün **içindekileri** sunucuya yükleyeceksiniz.

### 4.2 Plesk'te siteyi hazırlayın

1. **Websites & Domains** → alan adınız → **Hosting Settings**
2. Belge kökü (Document root): `httpdocs`
3. **ASP.NET Core** ayarları varsa etkinleştirin

### 4.3 Dosyaları yükleyin

`publish` içindekileri `httpdocs` klasörüne kopyalayın (Plesk Dosya Yöneticisi veya FTP).

> FTP kullanıyorsanız aktarım modunu **Binary** yapın; aksi hâlde DLL'ler bozulur.

### 4.4 Yazma izni verin

Bu iki klasörün yazılabilir olması gerekir:

| Klasör | Neden |
| --- | --- |
| `App_Data` | İçerik, ayarlar ve yedekler buraya yazılır |
| `wwwroot/uploads` | Panelden yüklenen görseller |
| `wwwroot/video` | Panelden yüklenen video |

Plesk → Dosya Yöneticisi → klasöre sağ tık → **Change Permissions** → uygulama havuzu kullanıcısına *Write* verin.

### 4.5 Kontrol edin

- `alanadiniz.com` → site açılmalı
- `alanadiniz.com/Panel/` → kurulum ekranı gelmeli
- `alanadiniz.com/App_Data/content.json` → **404 vermeli**. İçerik görünüyorsa `web.config` yüklenmemiştir.

### Uygulama açılmazsa

`web.config` içinde `stdoutLogEnabled="false"` değerini `"true"` yapın, siteyi bir kez açın, sonra `logs\stdout*.log` dosyasını okuyun. Hata orada yazar. Sorun çözülünce tekrar `"false"` yapın.

---

## 5. Teklif formu e-postası

Panel → **Şifre ve E-posta** bölümünden ayarlanır. Bilgileri hosting panelinizden alırsınız (E-posta Hesapları → Bağlantı Ayarları):

| Alan | Tipik değer |
| --- | --- |
| SMTP sunucusu | `mail.memetoglutasimacilik.com` |
| Port | 587 (STARTTLS) ya da 465 (SSL) |
| Kullanıcı adı | E-posta adresinin tamamı |
| Şifre | O e-posta hesabının şifresi |

Kaydettikten sonra **"Test postası gönder"** düğmesine basın.

**Önemli:** Gönderen adresi kendi alan adınıza ait olmalı. `@gmail.com` gibi bir adresten gönderirseniz postalar spam'a düşer.

**Talep kaybolmaz:** E-posta gönderilemezse ziyaretçiye "telefonla arayın" mesajı gösterilir ve talep `App_Data/talepler.log` dosyasına yazılır. Bu dosyayı ara sıra kontrol edin.

---

## 6. Hero videosu

Üst bölümdeki tam ekran video `wwwroot/video/hero.mp4` dosyasından gelir.

**Şu an kullanılan video:** Masaüstünüzdeki iki Pexels klibinden kurgulandı —
depoda tıra forkliftle yükleme yapılan sahne ve depo içi raf görüntüsü. İkisi
birbirine yumuşak geçişle bağlandı, sonu da başına eritildi; bu yüzden döngüde
kesik görünmez. 15,5 saniye, sesi yok.

| Dosya | Boyut |
| --- | --- |
| `wwwroot/video/hero.webm` | 2,0 MB (tarayıcı bunu tercih eder) |
| `wwwroot/video/hero.mp4` | 2,9 MB (yedek) |
| `wwwroot/img/hero-poster.jpg` | 175 KB |

> **Telif:** Pexels lisansı ticari kullanıma açıktır, atıf zorunlu değildir.
> Kaynak dosyalar Masaüstündeki `Memetoğlu Taşımacılık` klasöründe duruyor.

Panel → **Anasayfa → Üst bölüm → Video dosyası** üzerinden değiştirirsiniz.

| Ölçüt | Hedef |
| --- | --- |
| Süre | 10–20 saniye (döngüye girer) |
| Çözünürlük | 1280×720 yeterli |
| Dosya boyutu | **5 MB altı** — en önemlisi |

Site, **mobil cihazlarda videoyu hiç indirmez**, yalnızca poster görselini gösterir. Bu bilinçli bir tercihtir: mobil veri harcanmaz ve sayfa çok daha hızlı açılır.

### Video nereden bulunur

En iyisi kendi çekiminiz — yolda giden aracın 10-15 saniyelik telefon videosu yeter.

Ticari kullanıma açık ücretsiz stok kaynakları:

- <https://www.pexels.com/search/videos/logistics%20truck/>
- <https://pixabay.com/videos/search/truck/>
- <https://coverr.co/s?q=delivery>

> **Dikkat:** Başka bir lojistik firmasının (Ekol, DSV vb.) reklam filmini kullanmayın — telif ihlali olur.

### WebM (teknik not)

`wwwroot/video/` içinde MP4 ile **aynı adı taşıyan** bir `.webm` varsa site onu tercih eder (daha küçük dosya). Şu an `hero.mp4` ve `hero.webm` birlikte bulunuyor. Panelden yeni video yüklerseniz yalnızca MP4 olur; bu da sorunsuz çalışır.

---

## 7. SSL, alan adı ve Google

### SSL

Plesk → **SSL/TLS Sertifikaları** → *Let's Encrypt* (ücretsiz).

Sertifika kurulduktan sonra `web.config` içindeki yorum satırına alınmış `<rewrite>` bloğunu etkinleştirin — site HTTPS'e yönlenir.

### Alan adı

DNS panelinde `@` ve `www` için A kaydını hosting IP'nize yönlendirin. Sonra panelden
**Site Ayarları → SEO → Sitenin canlı adresi** alanını gerçek adresle güncelleyin (sonunda eğik çizgi olmadan).

### Google

1. **Site Ayarları → SEO** bölümünü doldurun (başlık 60, açıklama 155 karakteri geçmesin)
2. [Search Console](https://search.google.com/search-console)'a siteyi ekleyip `sitemap.xml` gönderin
3. **Google İşletme Profili** açın — yerel aramada ("Sultanbeyli nakliyat") çıkmanın en etkili yolu

`robots.txt` ve `sitemap.xml` kod tarafından üretilir; panelde girdiğiniz adresi kullanırlar.

---

## 8. Güvenlik

Kurulumda alınmış önlemler:

- Şifre **PBKDF2-SHA256** (210.000 tur) ile özetlenir; düz şifre hiçbir yerde saklanmaz
- Şifre karşılaştırması sabit zamanlıdır
- Aynı IP'den 5 hatalı girişten sonra 15 dakika bekleme
- Tüm formlarda antiforgery (CSRF) anahtarı — Razor Pages otomatik ekler
- Yüklenen dosyalar **uzantıya değil, ilk baytlarına** bakılarak doğrulanır; dosya adı yeniden üretilir
- `App_Data` klasörü tarayıcıdan erişime kapalıdır
- Razor tüm çıktıyı otomatik HTML kaçışından geçirir
- Güvenlik başlıkları (nosniff, X-Frame-Options, Referrer-Policy) eklenir

Size düşenler:

- [ ] Panel şifresini kimseyle paylaşmayın
- [ ] SSL'i mutlaka kurun
- [ ] `App_Data/config.json` dosyasını git'e göndermeyin (`.gitignore`'da tanımlı)
- [ ] `App_Data/content.json` dosyasının bir kopyasını ara sıra indirin

---

## 9. Proje yapısı

```
MemetogluWeb.csproj      Proje tanımı — hedef .NET sürümü burada
Program.cs               Uygulama kurulumu, kimlik doğrulama, robots/sitemap
web.config               IIS/Plesk ayarları

Models/
  SiteContent.cs         İçerik modeli — content.json ile birebir eşleşir
  AdminConfig.cs         Yönetici ve e-posta ayarları
  AlanVm.cs              Panel form alanı görünüm modeli

Services/
  ContentService.cs      JSON oku/yaz, atomik kayıt, otomatik yedek
  MailService.cs         SMTP gönderimi
  UploadService.cs       Dosya yükleme ve doğrulama
  PasswordHasher.cs      PBKDF2 şifre özeti
  Yardimci.cs            Ortak yardımcılar (WhatsApp, telefon, medya yolu)

Pages/
  Index.cshtml(.cs)      Anasayfa ve teklif formu
  Hata.cshtml(.cs)       Hata sayfası
  Shared/_Layout.cshtml  Menü, footer, SEO etiketleri
  Shared/Sections/       Sayfa bölümleri (hero, hizmetler, filo, kapsama…)
  Panel/                 Yönetim paneli ekranları

wwwroot/
  css/style.css          Sitenin tüm stilleri — renkler en üstte
  css/panel.css          Panel stilleri
  js/app.js              Animasyonlar, menü, video
  js/panel.js            Panel form yardımcıları
  img/  video/  uploads/

App_Data/
  content.json           SİTENİN TÜM İÇERİĞİ — en önemli dosya
  config.json            Şifre ve posta ayarları (kurulumda üretilir)
  yedek/                 Otomatik yedekler
```

### Renkleri değiştirmek

`wwwroot/css/style.css` başındaki `:root` bloğu:

```css
--accent: #d8121c;   /* kırmızı — butonlar, vurgular */
--navy:   #22267f;   /* lacivert — başlıklar */
--deep:   #10132c;   /* koyu bantlar ve footer */
```

### Kapsama haritası

`Pages/Shared/Sections/_Coverage.cshtml` içindeki `sehirler` dizisinden şehir eklenip çıkarılabilir.

Bu **ölçekli bir harita değildir** — noktalar gerçek enlem/boylamlardan türetilmiş şematik konumlardır, ülke sınırı yoktur.

---

## 10. Doğrulanan durum

Proje bu bilgisayarda derlendi ve çalıştırıldı:

- `dotnet build` → **0 hata, 0 uyarı**
- `http://localhost:5080` → anasayfa açıldı; tam ekran video, menü, butonlar,
  kaydırma animasyonları ve WhatsApp butonu tarayıcıda görülerek doğrulandı
- `http://localhost:5080/Panel/` → kurulum ekranına yönlendi
- `http://localhost:5080/sitemap.xml` → geçerli XML döndü

Ayrıca kod yazılırken yapılan denetimler:

- Panel formlarındaki **99 alanın tamamının** ilgili handler tarafından okunduğu
  doğrulandı (form alanı ↔ kod anahtarı eşleşmesi)
- Tüm JSON dosyalarının geçerliliği kontrol edildi

### Görsel önbelleği (teknik not)

Görsel ve video adreslerinin sonuna dosyanın değiştirilme zamanından üretilen
`?v=...` damgası eklenir. Böylece dosyalar tarayıcıda 30 gün önbellekte
kalabilirken, panelden bir görseli değiştirdiğinizde ziyaretçi **yenisini
anında** görür. Eski sürümü görme diye bir sorun yaşanmaz.

### Telefon numarası notu

Elinizdeki tanıtım görsellerinin birinde `0538 612 90 90`, diğer üçünde `0538 612 90 91` yazıyor. Projede **91** kullanıldı. Doğrusu farklıysa Site Ayarları → İletişim bölümünden düzeltin.
