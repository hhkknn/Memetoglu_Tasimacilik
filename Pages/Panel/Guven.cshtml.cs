using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Güven rozetleri, müşteri yorumları ve sık sorulan soruların düzenlendiği ekran.</summary>
public sealed class GuvenModel : PageModel
{
    private readonly ContentService _icerik;

    public GuvenModel(ContentService icerik) => _icerik = icerik;

    public SiteContent Icerik { get; private set; } = new();

    public void OnGet() => Icerik = _icerik.Read();

    public async Task<IActionResult> OnPostAsync()
    {
        var icerik = _icerik.Read();
        var f = Request.Form;

        // ------------------------------------------------------------ Güven rozetleri
        var rBaslik = f["rozet_baslik"];
        var rAciklama = f["rozet_aciklama"];
        var rIkon = f["rozet_ikon"];
        var rozetler = new List<TrustBadge>();
        for (var i = 0; i < rBaslik.Count; i++)
        {
            var b = (rBaslik[i] ?? "").Trim();
            if (b.Length == 0) { continue; }
            rozetler.Add(new TrustBadge
            {
                Title = b,
                Description = (i < rAciklama.Count ? rAciklama[i] : null)?.Trim() ?? "",
                Icon = (i < rIkon.Count ? rIkon[i] : null)?.Trim() is { Length: > 0 } ik ? ik : "shield",
            });
        }

        icerik.Trust = new TrustBlock
        {
            Enabled = IndexModel.Kutu(f, "rozet_acik"),
            NoticeEnabled = IndexModel.Kutu(f, "rozet_uyari_acik"),
            NoticeText = IndexModel.Metin(f, "rozet_uyari_metin"),
            Badges = rozetler,
        };

        // ------------------------------------------------------------ Yorumlar
        var yAlinti = f["yorum_alinti"];
        var yAd = f["yorum_ad"];
        var yUnvan = f["yorum_unvan"];
        var yYildiz = f["yorum_yildiz"];
        var yorumlar = new List<Testimonial>();
        for (var i = 0; i < yAlinti.Count; i++)
        {
            var a = (yAlinti[i] ?? "").Trim();
            if (a.Length == 0) { continue; }
            var yildiz = int.TryParse(i < yYildiz.Count ? yYildiz[i] : null, out var y) ? Math.Clamp(y, 1, 5) : 5;
            yorumlar.Add(new Testimonial
            {
                Quote = a,
                Name = (i < yAd.Count ? yAd[i] : null)?.Trim() ?? "",
                Role = (i < yUnvan.Count ? yUnvan[i] : null)?.Trim() ?? "",
                Stars = yildiz,
            });
        }

        icerik.Testimonials = new TestimonialsBlock
        {
            Enabled = IndexModel.Kutu(f, "yorum_acik"),
            Eyebrow = IndexModel.Metin(f, "yorum_eyebrow"),
            Title = IndexModel.Metin(f, "yorum_baslik"),
            Description = IndexModel.Metin(f, "yorum_aciklama"),
            NoticeEnabled = IndexModel.Kutu(f, "yorum_uyari_acik"),
            NoticeText = IndexModel.Metin(f, "yorum_uyari_metin"),
            GoogleRating = IndexModel.Metin(f, "google_puan"),
            GoogleReviewCount = IndexModel.Metin(f, "google_adet"),
            GoogleUrl = IndexModel.Metin(f, "google_url"),
            Items = yorumlar,
        };

        // ------------------------------------------------------------ SSS
        var sSoru = f["sss_soru"];
        var sCevap = f["sss_cevap"];
        var sorular = new List<FaqItem>();
        for (var i = 0; i < sSoru.Count; i++)
        {
            var q = (sSoru[i] ?? "").Trim();
            if (q.Length == 0) { continue; }
            sorular.Add(new FaqItem
            {
                Question = q,
                Answer = (i < sCevap.Count ? sCevap[i] : null)?.Trim() ?? "",
            });
        }

        icerik.Faq = new FaqBlock
        {
            Enabled = IndexModel.Kutu(f, "sss_acik"),
            Eyebrow = IndexModel.Metin(f, "sss_eyebrow"),
            Title = IndexModel.Metin(f, "sss_baslik"),
            Description = IndexModel.Metin(f, "sss_aciklama"),
            Items = sorular,
        };

        var ok = await _icerik.WriteAsync(icerik);

        TempData["Bildirim"] = ok
            ? "Güven, yorum ve SSS içerikleri kaydedildi."
            : "Kaydedilemedi. App_Data klasörünün yazma izni olduğundan emin olun.";
        TempData["BildirimTur"] = ok ? "ok" : "hata";

        return RedirectToPage("/Panel/Guven");
    }
}
