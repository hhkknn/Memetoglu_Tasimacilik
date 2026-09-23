using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Anasayfadaki kayan referans logoları şeridinin düzenlendiği ekran.</summary>
public sealed class ReferanslarModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly UploadService _yukleme;

    public ReferanslarModel(ContentService icerik, UploadService yukleme)
    {
        _icerik = icerik;
        _yukleme = yukleme;
    }

    public SiteContent Icerik { get; private set; } = new();

    public void OnGet() => Icerik = _icerik.Read();

    public async Task<IActionResult> OnPostAsync()
    {
        var icerik = _icerik.Read();
        var eski = icerik.References.Items;
        var f = Request.Form;
        var hatalar = new List<string>();
        var yeni = new List<ReferenceLogo>();

        var adlar = f["ad"];
        var baglantilar = f["url"];

        for (var i = 0; i < adlar.Count; i++)
        {
            if (f[$"sil_{i}"].Any(v => v == "true")) { continue; }

            var ad = (adlar[i] ?? "").Trim();
            if (ad.Length == 0) { continue; }

            var (yeniLogo, hata) = await _yukleme.SaveImageAsync(f.Files[$"logo_{i}_dosya"]);
            if (hata is not null) { hatalar.Add(hata); }

            var logo = yeniLogo ?? f[$"logo_{i}"].FirstOrDefault() ?? (i < eski.Count ? eski[i].Logo : "");
            if (f[$"logokaldir_{i}"].Any(v => v == "true")) { logo = ""; }

            yeni.Add(new ReferenceLogo
            {
                Name = ad,
                Logo = logo,
                Url = (i < baglantilar.Count ? baglantilar[i] : null)?.Trim() ?? "",
            });
        }

        var yeniAd = IndexModel.Metin(f, "yeni_ad");
        if (yeniAd.Length > 0)
        {
            var (yeniLogo, hata) = await _yukleme.SaveImageAsync(f.Files["yeni_logo_dosya"]);
            if (hata is not null) { hatalar.Add(hata); }
            yeni.Add(new ReferenceLogo { Name = yeniAd, Logo = yeniLogo ?? "", Url = IndexModel.Metin(f, "yeni_url") });
        }

        icerik.References = new ReferencesBlock
        {
            Enabled = IndexModel.Kutu(f, "ref_acik"),
            Title = IndexModel.Metin(f, "ref_baslik"),
            NoticeEnabled = IndexModel.Kutu(f, "ref_uyari_acik"),
            NoticeText = IndexModel.Metin(f, "ref_uyari_metin"),
            Items = yeni,
        };

        var ok = await _icerik.WriteAsync(icerik);

        TempData["Bildirim"] = ok
            ? (hatalar.Count > 0 ? "Kaydedildi, ama: " + string.Join(" ", hatalar) : "Referanslar kaydedildi.")
            : "Kaydedilemedi. App_Data klasörünün yazma izni olduğundan emin olun.";
        TempData["BildirimTur"] = ok && hatalar.Count == 0 ? "ok" : "hata";

        return RedirectToPage("/Panel/Referanslar");
    }
}
