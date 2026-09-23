# DEVİR NOTU — Memetoğlu Taşımacılık Web Sitesi

> Bu dosya, projeyi devralacak yeni bir Claude oturumu (ya da yeni bir geliştirici)
> için yazıldı. Hiçbir şey sormadan devam edebilmek için gereken bağlam burada.
> Kurulum ve yayına alma adımları ayrı dosyada: **`KURULUM.md`**.
>
> Son güncelleme: 23.09.2026

---

## 1. Tek cümleyle

Sultanbeyli'deki **Memetoğlu Taşımacılık** (karayolu nakliyat ve lojistik) için
yönetim panelli kurumsal web sitesi. **ASP.NET Core 10 Razor Pages**, veritabanı yok,
NuGet paketi yok, paylaşımlı hostingde (Plesk/IIS) yayınlanacak.

Proje sahibi: Hakan Yıldız — AIF Team (.NET/SAP danışmanlığı).
Site henüz yayında değil, alan adı ve hosting **alınmadı**.

---

## 2. Şu anki durum

**Çalışıyor.** Bu bilgisayarda derlendi ve tarayıcıda test edildi:

| Kontrol | Sonuç |
| --- | --- |
| `dotnet build` | 0 hata, 0 uyarı |
| Anasayfa `http://localhost:5080` | Açıldı; tam ekran video, menü, kaydırma animasyonları, WhatsApp butonu görüldü |
| `/Panel/` | Kurulum ekranına yönlendi |
| `/sitemap.xml` | Geçerli XML |
| Teklif formu | **Uçtan uca denendi.** Doğrulama geçti, SMTP tanımlı olmadığı için e-posta gitmedi, talep `App_Data/talepler.log` dosyasına düştü — yani hiçbir talep kaybolmuyor. Tasarlandığı gibi çalıştı. |

**Henüz denenmedi:** Panel giriş → düzenleme → kaydetme akışı. Yönetici şifresi
belirlenmediği için (`App_Data/config.json` oluşmadı) kurulum ekranından öteye geçilmedi.
**Devralan oturumun ilk işi bu olmalı.**

---

## 3. Yığın ve nedenleri — *burası önemli*

**Neden ASP.NET Core:** Kullanıcının ilk içgüdüsü C#'tı. Ben paylaşımlı hosting
kaygısıyla önce Next.js, sonra PHP önerdim; ikisi de yanlış çıktı. Makinede
Visual Studio, IIS, .NET SDK 10 ve 9, SQL Server kurulu — PHP/XAMPP **yok**.
Kullanıcı kendi alanına döndü. **Yığını tekrar sorgulamayın.**

**Denenip elenen yollar** (yeniden önermeyin):

| Yol | Neden elendi |
| --- | --- |
| Next.js 15 + Payload CMS | Kurumun npm çıkışı kapalı — **tüm** paketlerde 403. Hiç derlenemedi. |
| PHP 8.4 + özel panel | Tamamen yazıldı ve test edildi, ama makinede PHP yok. |
| WordPress | Kullanıcı açıkça istemedi. |
| VPS / kendi sunucu | Bütçe yok. Yalnızca cPanel/Plesk paylaşımlı hosting. |

**Tasarım kararları:**

- **Sıfır NuGet paketi** — yalnızca .NET çerçevesi. Paket sürüm çakışması riski yok.
- **Veritabanı yok** — tüm içerik `App_Data/content.json`. Yedek = dosyayı kopyala.
- Kimlik doğrulama: çerez tabanlı, **PBKDF2-SHA256, 210.000 tur**.
- Yükleme güvenliği: dosya uzantısına değil **magic byte**'a bakılır; dosya adı baştan üretilir.
- Referans alınan siteler: flexport.com (düzen), ekol.com + dsv.com (tam ekran video hero),
  muratlojistik.com.tr (kaydırma animasyonları).

**Telif kuralı — uyulmaya devam edilmeli:** Kullanıcı bir ara "yabancı sitedeki videoyu
direkt kullan" dedi; reddedildi. Şu anki video kullanıcının kendi indirdiği
**Pexels** kliplerinden kurgulandı (ticari kullanıma açık lisans). Başka bir lojistik
firmasının reklam filmi **kullanılmayacak**.

---

## 4. Bu bilgisayara özgü iki tuzak

**1. Windows Uygulama Denetimi (WDAC/AppLocker)**
Kurumsal ilke, kendi derlediğiniz imzasız `MemetogluWeb.exe`'yi çalıştırmayı engelliyor
("Uygulama Denetimi ilkesi bu dosyayı engelledi"). Kodda sorun yoktur.
Çözüm — Microsoft imzalı `dotnet.exe` ile DLL'i çalıştırın:

```powershell
dotnet bin\Debug\net10.0\MemetogluWeb.dll
```

Proje kökündeki **`hepsi.bat`** bunu otomatik yapar (5080'deki eski süreci kapatır,
derler, başlatır, curl ile sağlık kontrolü yapar, `calisma-log.txt`'ye yazar).
Yayın sunucusunda bu kısıt yoktur.

**2. Terminale yazı yazılamıyor**
Bu ortamda terminal/IDE pencerelerine klavye girişi yapılamıyor (yalnızca tıklama).
Bu yüzden her komut bir `.bat` dosyasına yazılıp çift tıklanarak çalıştırıldı ve
çıktı log dosyasından okundu. `derle.bat`, `derle2.bat`, `calistir.bat`, `test.bat`,
`test2.bat`, `hepsi.bat` bu yüzden var. **`hepsi.bat` hepsinin yerine geçer**,
diğerleri silinebilir.

---

## 5. Proje haritası

```
MemetogluWeb/
├─ Program.cs                 Razor Pages, çerez auth, /Panel yetkilendirme,
│                             statik dosya önbelleği, güvenlik başlıkları,
│                             minimal API ile /robots.txt ve /sitemap.xml
├─ MemetogluWeb.csproj        net10.0, paket yok
├─ web.config                 IIS/Plesk için
│
├─ Models/
│  ├─ SiteContent.cs          content.json'un birebir C# karşılığı
│  ├─ AdminConfig.cs          yönetici e-postası, şifre özeti, SMTP (config.json)
│  └─ AlanVm.cs               panel form alanı görünüm modeli
│
├─ Services/
│  ├─ ContentService.cs       atomik yazma (geçici dosya + taşı), App_Data/yedek/
│  │                          içine son 20 yedek, SemaphoreSlim kilidi,
│  │                          LogQuoteAsync (e-posta gitmezse talebi kaydeder)
│  ├─ PasswordHasher.cs       PBKDF2-SHA256, "tur.saltB64.hashB64", sabit süreli karşılaştırma
│  ├─ UploadService.cs        magic byte doğrulama (JPEG/PNG/GIF/WEBP, MP4/WEBM)
│  ├─ MailService.cs          System.Net.Mail.SmtpClient + HTML tablo üretici
│  └─ Yardimci.cs             Medya() yol yardımcısı (sürüm damgası dahil),
│                             WhatsAppUrl(), TelHref(), SVG ikon yolları
│
├─ Pages/
│  ├─ Index.cshtml(.cs)       anasayfa + teklif formu POST
│  ├─ Hata.cshtml
│  ├─ Shared/_Layout.cshtml
│  ├─ Shared/Sections/        _Hero _Stats _Services _Solutions _Statement
│  │                          _Fleet _Coverage _Process _About _Contact
│  └─ Panel/                  Kurulum Giris Index Ayarlar Hizmetler Filo
│                             Surec Medya Sifre Cikis + _Alan + _PanelLayout
│
├─ App_Data/
│  ├─ content.json            TÜM site içeriği
│  ├─ config.json             kurulumda oluşur — GİT'E GİRMEZ
│  ├─ talepler.log            e-posta gidemeyen teklif talepleri
│  └─ yedek/                  otomatik içerik yedekleri
│
└─ wwwroot/
   ├─ css/style.css           genel site
   ├─ css/panel.css           yönetim paneli
   ├─ js/app.js               kaydırma animasyonları, hero video, menü
   ├─ img/                    görseller + hero-poster.jpg
   ├─ video/hero.mp4 + .webm  tam ekran hero videosu
   └─ uploads/                panelden yüklenenler
```

### Bilinmesi gereken kod ayrıntıları

- **Panel formları indeks kullanmaz.** Tekrar eden satırlar (maddeler, rakamlar,
  bağlantılar) JavaScript ile eklenip silinebildiği için `Request.Form` üzerinden
  sırayla okunur — model binding değil. `Pages/Panel/Index.cshtml.cs` içindeki
  `Metin / Kutu / Liste / Ciftler / Baslik / GorselCoz` yardımcılarına bakın.
  Çözüm merkezi bağlantıları düz adlandırma kullanır: `sol_label_{i}` / `sol_href_{i}`.
- **Panel alanları `<partial>` tag helper'ı ile yazılamaz.** Razor, TagHelper
  öznitelik değerinin içindeki tırnakları ayrıştıramıyor. Hepsi
  `@await Html.PartialAsync("_Alan", new AlanVm { ... })` biçiminde (69 alan).
- **Sayfalarda açık rota yok.** Hepsi düz `@page`. Açık rota (`@page "/Panel/Index"`)
  yazarsanız `/Panel/` → `/Panel/Index` klasör varsayılanı bozulur ve panel 404 verir.
  **Bu hata bir kez yaşandı, tekrarlamayın.**
- **Medya adresleri sürüm damgalı.** `Yardimci.Medya()` dosyanın değiştirilme
  zamanından `?v=...` üretir. Böylece statik dosyalar 30 gün önbellekte kalırken
  panelden değiştirilen görsel anında görünür. `Yardimci.KokKlasor` Program.cs'te atanır.
- Razor'da `Path` özelliği `System.IO.Path`'i gölgeler — tam nitelendirin.
- SVG `<text>` etiketi Razor'ın `<text>` yönergesiyle çakışır — `<g>` ile sarın.
- `PageModel.Content(string)` diye bir metot var; içerik özelliğinin adı bu yüzden
  `Content` değil **`Icerik`**.

---

## 6. Düzeltilen hatalar — geri getirmeyin

| # | Hata | Kök neden | Çözüm |
| --- | --- | --- | --- |
| 1 | `/Panel/` 404 | Sayfalara açık rota verilmişti | Açık rotalar kaldırıldı, düz `@page` |
| 2 | Hero videosu oynamıyor gibi | Poster ve video üst üste değildi; video posterin 720 px **altına** akıyordu, ekran dışında oynuyordu | İkisi de `position:absolute;inset:0` |
| 3 | "Bu alanı boş bırakın" alanı ekranda görünüyor | `.honey` kuralı CSS'te hiç yoktu | Ekran dışına alındı (`display:none` değil — botlar gizli alanı atlar) |
| 4 | Slogan bandında dev boşluklar | Kural `.statement img.bgimg` yazılmış, sınıf ise saran `div`'in üzerindeydi → hiç eşleşmiyor, görsel doğal boyutta akışa giriyordu | `.statement .bgimg` + iç `img` |
| 5 | Form sonuç kutusu biçimsiz | `.form-msg` CSS'i yoktu (PHP sürümünden kalma `.form-ok` vardı) | `.form-msg.ok` / `.form-msg.err` |
| 6 | NETSDK1022 yinelenen öğe | `<Content Include=...>` | `<Content Update=...>` |
| 7 | Mobilde yatay kayma | `data-reveal="right"` öğeleri beklerken 48 px dışarıda duruyor | `body{overflow-x:clip}` (`hidden` değil — sticky'yi bozar) |

**Devir sırasında dikkat:** Bu ortamda dosyaları kullanıcının bilgisayarına
yazarken **aynı kaynak yolundan ikinci kez yazma sessizce eski içeriği
gönderebiliyor.** İki kez yaşandı. Her yazımdan sonra dosya boyutunu doğrulayın;
uyuşmuyorsa dosyayı yeni bir geçici klasöre kopyalayıp oradan gönderin.

---

## 7. Açık kalemler

1. **Panel akışı test edilmedi.** Yönetici şifresi belirlenmeli, sonra:
   giriş → metin düzenle → kaydet → görsel yükle → çıkış. En öncelikli iş.
2. **Telefon numarası çelişkisi.** Tanıtım görsellerinin birinde `0538 612 90 90`,
   diğer üçünde `0538 612 90 91`. Projede **91** kullanıldı. Kullanıcı teyit etmeli.
3. **Hosting alınmadı.** Plesk paketi .NET 10 desteklemiyorsa `<TargetFramework>`
   `net8.0` yapılacak ya da self-contained yayın alınacak. Kullanıcı sorulacak.
4. **Alan adı alınmadı.** `content.json` içinde
   `https://www.memetoglutasimacilik.com` varsayıldı (sitemap ve og:url bunu kullanır).
5. **SMTP bilgileri girilmedi.** Hosting alınınca panelden eklenecek.
   Gönderen adresi kendi alan adında olmalı, yoksa postalar spam'e düşer.
6. Kök klasördeki eski `.bat` ve `.txt` dosyaları temizlenebilir
   (`hepsi.bat` dışında), `.vs/` klasörü de.

---

## 8. Masaüstündeki diğer klasörler

`C:\Users\AIFTeam-13\Desktop\Memetoğlu Taşımacılık\` içinde:

| Klasör / dosya | Durum |
| --- | --- |
| `MemetogluWeb/` | **GÜNCEL PROJE** |
| `web-taslak/` | İlk statik HTML taslağı — eskidi |
| `memetoglu-web/` | Next.js + Payload denemesi — hiç derlenmedi, eskidi |
| `memetoglu-site/` + `.zip` | PHP sürümü — çalışıyordu ama terk edildi |
| `*.mp4` (3 adet) | Pexels stok klipleri. İkisi hero videosunda kullanıldı. |
| `WhatsApp Image *.jpeg` | Firmanın kendi araç ve tanıtım görselleri |

Kullanılmayan üçüncü klip (`20682093-hd_...`): bir çalışanın montunda **başka bir
firmanın logosu** görünüyor ve görüntü sallantılı — bilerek kullanılmadı.

---

## 9. Yeni oturuma ilk mesaj için öneri

> Memetoğlu Taşımacılık web sitesi projesini devralıyorsun.
> `MemetogluWeb` klasöründeki `DEVIR.md` ve `KURULUM.md` dosyalarını oku;
> projenin tüm bağlamı, verilmiş kararlar, düzeltilmiş hatalar ve açık kalemler orada.
> Proje ASP.NET Core 10 Razor Pages, derleniyor ve çalışıyor.
> Sıradaki iş: yönetim panelinin giriş–düzenle–kaydet akışını test etmek.

Ekte `MemetogluWeb.zip` var — `bin/`, `obj/` ve `.vs/` hariç temiz kaynak.
Açıp `hepsi.bat`'a çift tıklamak yeterli.

---

## 10. 2. oturum (23.09.2026) — bulut oturumunda yapılanlar

Bu oturum bulutta (Linux, .NET 10 SDK) çalıştı; kod GitHub'da
**`claude/hopeful-gates-l50x7u`** dalında. `master` dalı eski hâlde duruyor —
iş bitince bu dal `master`'a birleştirilmeli.

**Kullanıcının test yöntemi (önemli):** Masaüstünde `git clone` ile alınmış bir
kopya var; kullanıcı her değişiklikten sonra **`calistir.bat`**'a çift tıklıyor
(git pull → 5080'deki eski süreci kapat → `dotnet run` → tarayıcıyı aç).
Bu akış kullanıcının makinesinde çalıştığı doğrulandı. ZIP indirmeye gerek yok.

**Yapılanlar:**
- Panel giriş → düzenle → kaydet → yedek → çıkış akışı test edildi, hatasız (§7 madde 1 kapandı).
- `UseAppHost=false` (csproj): proje `.exe` üretmiyor; F5 ve `dotnet run` WDAC'a takılmıyor.
  §4'teki `.bat` dosyaları artık gereksiz; yerine repodaki `calistir.bat`.
- Hero videosu: kullanıcının verdiği panelvan yükleme klibi (Pexels, 4K → 1080p).
  Slogan bandına ikinci arka plan videosu (depo) eklendi; `StatementBlock.Video/VideoEnabled`,
  panelden yüklenebilir. JS artık `.bg-video` sınıflı tüm videoları yönetiyor.
- Hakkımızda'ya "Vizyonumuz" kutusu (`AboutBlock.Vision`), hizmetlere "Kiralık kamyonet".
- Yeni bölümler (hepsi panelden düzenlenir, hepsi **örnek veriyle** ve "örnek veri" etiketiyle):
  güven şeridi (`trust`), referans logoları kayan şeridi (`references`, Panel > Referanslar),
  müşteri yorumları + Google puanı (`testimonials`), SSS (`faq`, `<details>` ile, JS yok).
  Güven/yorum/SSS: Panel > Güven & Yorumlar.
- Sağdan açılan yan menü (ekol.com esinli). ≥1101px'te sayfa kalan alana **sığacak şekilde
  daralır** (kaydırılmaz — kullanıcı özellikle böyle istedi); daha dar ekranda üstüne açılır.
  Mobilde üst menünün taşması da bu sayede çözüldü.
- `Address.OneLine` artık `[JsonIgnore]`.

**Açık kalemler (yayından önce şart):**
1. **Örnek veriler gerçekleriyle değiştirilmeli:** K1 belge numarası (şu an "eklenecek"),
   Google puanı/yorum sayısı/bağlantısı, müşteri yorumları, referans logoları, rakamlar.
   Sahte yorum, puan ya da belge numarası **yayınlanmamalı**. Rakip firma logoları
   referans olarak **kullanılmayacak** (kullanıcı önerdi, reddedildi).
2. §7'deki telefon (90 90 / 90 91), hosting, alan adı ve SMTP kalemleri hâlâ açık.
3. Kullanıcıya önerilen ama yapılmayan: "Şehirler arası taşımacılık" kartındaki
   `roads.jpg` illüstrasyon (diğer kartlar fotoğraf); Çözüm Merkezi ile slogan bandı
   art arda iki koyu bölüm.
