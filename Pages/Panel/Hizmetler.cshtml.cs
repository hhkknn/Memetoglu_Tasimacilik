using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Hizmet kartlarının düzenlendiği ekran.</summary>
public sealed class HizmetlerModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly UploadService _yukleme;

    public HizmetlerModel(ContentService icerik, UploadService yukleme)
    {
        _icerik = icerik;
        _yukleme = yukleme;
    }

    public SiteContent Icerik { get; private set; } = new();

    public void OnGet() => Icerik = _icerik.Read();

    public async Task<IActionResult> OnPostAsync()
    {
        var icerik = _icerik.Read();
        var eski = icerik.Services;
        var f = Request.Form;
        var hatalar = new List<string>();
        var yeni = new List<ServiceCard>();

        var basliklar = f["baslik"];
        var aciklamalar = f["aciklama"];
        var altMetinler = f["alt"];
        var etiketler = f["etiketler"];

        for (var i = 0; i < basliklar.Count; i++)
        {
            // "Sil" işaretlenmiş kartı atla.
            if (f[$"sil_{i}"].Any(v => v == "true" || v == "on"))
            {
                continue;
            }

            var baslik = (basliklar[i] ?? "").Trim();
            if (baslik.Length == 0)
            {
                continue;
            }

            var mevcutGorsel = i < eski.Count ? eski[i].Image : "";
            var (yeniGorsel, hata) = await _yukleme.SaveImageAsync(f.Files[$"gorsel_{i}_dosya"]);
            if (hata is not null) { hatalar.Add(hata); }

            yeni.Add(new ServiceCard
            {
                Title = baslik,
                Description = (i < aciklamalar.Count ? aciklamalar[i] : null)?.Trim() ?? "",
                Image = yeniGorsel ?? (f[$"gorsel_{i}"].FirstOrDefault() ?? mevcutGorsel),
                Alt = (i < altMetinler.Count ? altMetinler[i] : null)?.Trim() ?? "",
                Tags = EtiketAyir(i < etiketler.Count ? etiketler[i] : null),
            });
        }

        // Yeni kart eklenmişse sona koy.
        var yeniBaslik = IndexModel.Metin(f, "yeni_baslik");
        if (yeniBaslik.Length > 0)
        {
            var (yeniGorsel, hata) = await _yukleme.SaveImageAsync(f.Files["yeni_gorsel_dosya"]);
            if (hata is not null) { hatalar.Add(hata); }

            yeni.Add(new ServiceCard
            {
                Title = yeniBaslik,
                Description = IndexModel.Metin(f, "yeni_aciklama"),
                Image = yeniGorsel ?? "",
                Alt = IndexModel.Metin(f, "yeni_alt"),
                Tags = EtiketAyir(f["yeni_etiketler"].FirstOrDefault()),
            });
        }

        icerik.Services = yeni;
        var ok = await _icerik.WriteAsync(icerik);

        TempData["Bildirim"] = ok
            ? (hatalar.Count > 0 ? "Kaydedildi, ama: " + string.Join(" ", hatalar) : "Hizmetler kaydedildi.")
            : "Kaydedilemedi. App_Data klasörünün yazma izni olduğundan emin olun.";
        TempData["BildirimTur"] = ok && hatalar.Count == 0 ? "ok" : "hata";

        return RedirectToPage("/Panel/Hizmetler");
    }

    /// <summary>Virgülle ayrılmış etiket metnini listeye çevirir.</summary>
    private static List<string> EtiketAyir(string? ham) =>
        (ham ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
}
