using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Firma bilgileri, iletişim, sosyal medya ve SEO ayarları.</summary>
public sealed class AyarlarModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly UploadService _yukleme;

    public AyarlarModel(ContentService icerik, UploadService yukleme)
    {
        _icerik = icerik;
        _yukleme = yukleme;
    }

    public SiteContent Icerik { get; private set; } = new();

    public static readonly (string Anahtar, string Etiket)[] Platformlar =
    {
        ("facebook", "Facebook"),
        ("instagram", "Instagram"),
        ("linkedin", "LinkedIn"),
        ("x", "X (Twitter)"),
        ("youtube", "YouTube"),
    };

    public void OnGet() => Icerik = _icerik.Read();

    public async Task<IActionResult> OnPostAsync()
    {
        var icerik = _icerik.Read();
        var eski = icerik.Settings;
        var f = Request.Form;
        var hatalar = new List<string>();

        // Telefonlar
        var telEtiket = f["tel_label"];
        var telArama = f["tel_dial"];
        var telefonlar = new List<Phone>();
        for (var i = 0; i < telEtiket.Count; i++)
        {
            var etiket = (telEtiket[i] ?? "").Trim();
            if (etiket.Length == 0) { continue; }
            var arama = (i < telArama.Count ? telArama[i] : null)?.Trim() ?? "";
            telefonlar.Add(new Phone { Label = etiket, Dial = arama.Length > 0 ? arama : etiket });
        }

        // Sosyal medya
        var sosPlatform = f["sos_platform"];
        var sosUrl = f["sos_url"];
        var sosyal = new List<Social>();
        for (var i = 0; i < sosPlatform.Count; i++)
        {
            var p = (sosPlatform[i] ?? "").Trim();
            if (p.Length == 0) { continue; }
            sosyal.Add(new Social
            {
                Platform = p,
                Url = (i < sosUrl.Count ? sosUrl[i] : null)?.Trim() ?? "",
            });
        }

        icerik.Settings = new Settings
        {
            CompanyName = IndexModel.Metin(f, "firma"),
            Slogan = IndexModel.Metin(f, "slogan"),
            Logo = await GorselCoz(f, "logo", eski.Logo, hatalar),
            Email = IndexModel.Metin(f, "email"),
            Phones = telefonlar,
            WhatsApp = new WhatsApp
            {
                Enabled = IndexModel.Kutu(f, "wa_acik"),
                Number = new string(IndexModel.Metin(f, "wa_numara").Where(char.IsAsciiDigit).ToArray()),
                Message = IndexModel.Metin(f, "wa_mesaj"),
            },
            Address = new Address
            {
                Line1 = IndexModel.Metin(f, "adr_satir"),
                District = IndexModel.Metin(f, "adr_ilce"),
                City = IndexModel.Metin(f, "adr_il"),
                PostalCode = IndexModel.Metin(f, "adr_posta"),
                MapsUrl = IndexModel.Metin(f, "adr_harita"),
            },
            WorkingHours = IndexModel.Liste(f, "saatler"),
            Socials = sosyal,
            Seo = new Seo
            {
                Title = IndexModel.Metin(f, "seo_baslik"),
                Description = IndexModel.Metin(f, "seo_aciklama"),
                SiteUrl = IndexModel.Metin(f, "seo_adres").TrimEnd('/'),
                ShareImage = await GorselCoz(f, "seo_gorsel", eski.Seo.ShareImage, hatalar),
            },
            DraftNotice = new Notice
            {
                Enabled = IndexModel.Kutu(f, "uyari_acik"),
                Text = IndexModel.Metin(f, "uyari_metin"),
            },
        };

        var ok = await _icerik.WriteAsync(icerik);

        TempData["Bildirim"] = ok
            ? (hatalar.Count > 0 ? "Kaydedildi, ama: " + string.Join(" ", hatalar) : "Site ayarları kaydedildi.")
            : "Kaydedilemedi. App_Data klasörünün yazma izni olduğundan emin olun.";
        TempData["BildirimTur"] = ok && hatalar.Count == 0 ? "ok" : "hata";

        return RedirectToPage("/Panel/Ayarlar");
    }

    private async Task<string> GorselCoz(IFormCollection f, string ad, string mevcut, List<string> hatalar)
    {
        var (yeni, hata) = await _yukleme.SaveImageAsync(f.Files[ad + "_dosya"]);
        if (hata is not null) { hatalar.Add(hata); }
        if (yeni is not null) { return yeni; }
        var gizli = f[ad].FirstOrDefault();
        return string.IsNullOrWhiteSpace(gizli) ? mevcut : gizli;
    }
}
