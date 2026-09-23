using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>
/// Anasayfa bölümlerinin düzenlendiği ekran.
///
/// Tekrar eden satırlar (maddeler, rakamlar, bağlantılar) dizi olarak gelir;
/// JavaScript ile satır eklenip silinebildiği için form alanları indeks
/// kullanmaz, Request.Form üzerinden sırayla okunur.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly UploadService _yukleme;

    public IndexModel(ContentService icerik, UploadService yukleme)
    {
        _icerik = icerik;
        _yukleme = yukleme;
    }

    public SiteContent Icerik { get; private set; } = new();

    public void OnGet()
    {
        Icerik = _icerik.Read();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var icerik = _icerik.Read();
        var f = Request.Form;
        var hatalar = new List<string>();

        // ------------------------------------------------------------ Hero
        var heroVideo = icerik.Hero.Video;
        var (yeniVideo, videoHata) = await _yukleme.SaveVideoAsync(f.Files["hero_video_dosya"]);
        if (videoHata is not null) { hatalar.Add(videoHata); }
        if (yeniVideo is not null) { heroVideo = yeniVideo; }

        icerik.Hero = new Hero
        {
            Eyebrow = Metin(f, "hero_eyebrow"),
            HeadlineTop = Metin(f, "hero_top"),
            HeadlineBottom = Metin(f, "hero_bottom"),
            Subtitle = Metin(f, "hero_subtitle"),
            PrimaryButton = Metin(f, "hero_button"),
            VideoEnabled = Kutu(f, "hero_video_acik"),
            Video = heroVideo,
            Poster = await GorselCoz(f, "hero_poster", icerik.Hero.Poster, hatalar),
            Highlights = Liste(f, "hero_highlights"),
        };

        // ------------------------------------------------------------ Rakamlar
        icerik.Stats = new StatsBlock
        {
            Enabled = Kutu(f, "stats_acik"),
            Eyebrow = Metin(f, "stats_eyebrow"),
            NoticeEnabled = Kutu(f, "stats_uyari_acik"),
            NoticeText = Metin(f, "stats_uyari"),
            Items = Ciftler(f, "stat_value", "stat_label"),
        };

        // ------------------------------------------------------------ Bölüm başlıkları
        icerik.ServicesSection = Baslik(f, "svc");
        icerik.FleetSection = Baslik(f, "flt");
        icerik.ProcessSection = Baslik(f, "prc");

        // ------------------------------------------------------------ Çözüm merkezi
        var sutunlar = new List<SolutionColumn>();
        var basliklar = f["sol_heading"];
        for (var i = 0; i < basliklar.Count; i++)
        {
            var baslik = (basliklar[i] ?? "").Trim();
            if (baslik.Length == 0) { continue; }

            var etiketler = f[$"sol_label_{i}"];
            var hedefler = f[$"sol_href_{i}"];
            var baglantilar = new List<SolutionLink>();

            for (var k = 0; k < etiketler.Count; k++)
            {
                var etiket = (etiketler[k] ?? "").Trim();
                if (etiket.Length == 0) { continue; }
                var hedef = (k < hedefler.Count ? hedefler[k] : null)?.Trim();
                baglantilar.Add(new SolutionLink
                {
                    Label = etiket,
                    Href = string.IsNullOrWhiteSpace(hedef) ? "#iletisim" : hedef,
                });
            }

            sutunlar.Add(new SolutionColumn { Heading = baslik, Links = baglantilar });
        }

        icerik.Solutions = new SolutionsBlock
        {
            Enabled = Kutu(f, "sol_acik"),
            Eyebrow = Metin(f, "sol_eyebrow"),
            Title = Metin(f, "sol_title"),
            Description = Metin(f, "sol_desc"),
            Columns = sutunlar,
        };

        // ------------------------------------------------------------ Slogan bandı
        var stmVideo = icerik.Statement.Video;
        var (yeniStmVideo, stmVideoHata) = await _yukleme.SaveVideoAsync(f.Files["stm_video_dosya"], "statement");
        if (stmVideoHata is not null) { hatalar.Add(stmVideoHata); }
        if (yeniStmVideo is not null) { stmVideo = yeniStmVideo; }

        icerik.Statement = new StatementBlock
        {
            Enabled = Kutu(f, "stm_acik"),
            Quote = Metin(f, "stm_quote"),
            Description = Metin(f, "stm_desc"),
            ButtonLabel = Metin(f, "stm_button"),
            Image = await GorselCoz(f, "stm_image", icerik.Statement.Image, hatalar),
            VideoEnabled = Kutu(f, "stm_video_acik"),
            Video = stmVideo,
        };

        // ------------------------------------------------------------ Kapsama
        icerik.Coverage = new CoverageBlock
        {
            Enabled = Kutu(f, "cov_acik"),
            Eyebrow = Metin(f, "cov_eyebrow"),
            Title = Metin(f, "cov_title"),
            Description = Metin(f, "cov_desc"),
            Figures = Ciftler(f, "cov_value", "cov_label"),
        };

        // ------------------------------------------------------------ Hakkımızda
        var maddeler = new List<Feature>();
        var mBaslik = f["abt_f_title"];
        var mAciklama = f["abt_f_desc"];
        for (var i = 0; i < mBaslik.Count; i++)
        {
            var b = (mBaslik[i] ?? "").Trim();
            var a = (i < mAciklama.Count ? mAciklama[i] : null)?.Trim() ?? "";
            if (b.Length > 0 || a.Length > 0)
            {
                maddeler.Add(new Feature { Title = b, Description = a });
            }
        }

        icerik.About = new AboutBlock
        {
            Eyebrow = Metin(f, "abt_eyebrow"),
            Title = Metin(f, "abt_title"),
            Description = Metin(f, "abt_desc"),
            Image = await GorselCoz(f, "abt_image", icerik.About.Image, hatalar),
            Alt = Metin(f, "abt_alt"),
            Features = maddeler,
        };

        // ------------------------------------------------------------ İletişim
        icerik.Contact = new ContactBlock
        {
            Eyebrow = Metin(f, "ctc_eyebrow"),
            Title = Metin(f, "ctc_title"),
            Description = Metin(f, "ctc_desc"),
            FormTitle = Metin(f, "ctc_form_title"),
            FormNote = Metin(f, "ctc_form_note"),
        };

        var ok = await _icerik.WriteAsync(icerik);

        TempData["Bildirim"] = ok
            ? (hatalar.Count > 0
                ? "Kaydedildi, ama bazı dosyalar yüklenemedi: " + string.Join(" ", hatalar)
                : "Anasayfa içeriği kaydedildi.")
            : "Kaydedilemedi. App_Data klasörünün yazma izni olduğundan emin olun.";
        TempData["BildirimTur"] = ok && hatalar.Count == 0 ? "ok" : "hata";

        return RedirectToPage("/Panel/Index");
    }

    // ------------------------------------------------------------ Yardımcılar

    internal static string Metin(IFormCollection f, string ad) => (f[ad].FirstOrDefault() ?? "").Trim();

    internal static bool Kutu(IFormCollection f, string ad) =>
        f[ad].Any(v => v == "true" || v == "on" || v == "1");

    internal static List<string> Liste(IFormCollection f, string ad) =>
        f[ad].Select(v => (v ?? "").Trim()).Where(v => v.Length > 0).ToList();

    internal static List<ValueLabel> Ciftler(IFormCollection f, string degerAd, string etiketAd)
    {
        var degerler = f[degerAd];
        var etiketler = f[etiketAd];
        var cikti = new List<ValueLabel>();

        for (var i = 0; i < degerler.Count; i++)
        {
            var d = (degerler[i] ?? "").Trim();
            var e = (i < etiketler.Count ? etiketler[i] : null)?.Trim() ?? "";
            if (d.Length > 0 || e.Length > 0)
            {
                cikti.Add(new ValueLabel { Value = d, Label = e });
            }
        }
        return cikti;
    }

    private static SectionHeading Baslik(IFormCollection f, string on) => new()
    {
        Eyebrow = Metin(f, on + "_eyebrow"),
        Title = Metin(f, on + "_title"),
        Description = Metin(f, on + "_desc"),
    };

    /// <summary>Yeni görsel yüklendiyse onun yolunu, yoksa mevcut yolu döndürür.</summary>
    private async Task<string> GorselCoz(IFormCollection f, string ad, string mevcut, List<string> hatalar)
    {
        var (yeni, hata) = await _yukleme.SaveImageAsync(f.Files[ad + "_dosya"]);
        if (hata is not null) { hatalar.Add(hata); }
        if (yeni is not null) { return yeni; }

        var gizli = f[ad].FirstOrDefault();
        return string.IsNullOrWhiteSpace(gizli) ? mevcut : gizli;
    }
}
