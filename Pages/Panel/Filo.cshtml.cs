using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Filo kartlarının düzenlendiği ekran.</summary>
public sealed class FiloModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly UploadService _yukleme;

    public FiloModel(ContentService icerik, UploadService yukleme)
    {
        _icerik = icerik;
        _yukleme = yukleme;
    }

    public SiteContent Icerik { get; private set; } = new();

    public void OnGet() => Icerik = _icerik.Read();

    public async Task<IActionResult> OnPostAsync()
    {
        var icerik = _icerik.Read();
        var eski = icerik.Fleet;
        var f = Request.Form;
        var hatalar = new List<string>();
        var yeni = new List<FleetCard>();

        var basliklar = f["baslik"];
        var altMetinler = f["alt"];

        for (var i = 0; i < basliklar.Count; i++)
        {
            if (f[$"sil_{i}"].Any(v => v == "true" || v == "on"))
            {
                continue;
            }

            var baslik = (basliklar[i] ?? "").Trim();
            if (baslik.Length == 0)
            {
                continue;
            }

            // Özellik satırları sütun bazlı adlarla gelir: oz_label_0, oz_value_0 ...
            var ozEtiket = f[$"oz_label_{i}"];
            var ozDeger = f[$"oz_value_{i}"];
            var ozellikler = new List<Spec>();

            for (var k = 0; k < ozEtiket.Count; k++)
            {
                var e = (ozEtiket[k] ?? "").Trim();
                var d = (k < ozDeger.Count ? ozDeger[k] : null)?.Trim() ?? "";
                if (e.Length > 0 || d.Length > 0)
                {
                    ozellikler.Add(new Spec { Label = e, Value = d });
                }
            }

            var mevcutGorsel = i < eski.Count ? eski[i].Image : "";
            var (yeniGorsel, hata) = await _yukleme.SaveImageAsync(f.Files[$"gorsel_{i}_dosya"]);
            if (hata is not null) { hatalar.Add(hata); }

            yeni.Add(new FleetCard
            {
                Title = baslik,
                Image = yeniGorsel ?? (f[$"gorsel_{i}"].FirstOrDefault() ?? mevcutGorsel),
                Alt = (i < altMetinler.Count ? altMetinler[i] : null)?.Trim() ?? "",
                Specs = ozellikler,
            });
        }

        var yeniBaslik = IndexModel.Metin(f, "yeni_baslik");
        if (yeniBaslik.Length > 0)
        {
            var (yeniGorsel, hata) = await _yukleme.SaveImageAsync(f.Files["yeni_gorsel_dosya"]);
            if (hata is not null) { hatalar.Add(hata); }

            yeni.Add(new FleetCard
            {
                Title = yeniBaslik,
                Image = yeniGorsel ?? "",
                Alt = IndexModel.Metin(f, "yeni_alt"),
                Specs = new List<Spec>(),
            });
        }

        icerik.Fleet = yeni;
        var ok = await _icerik.WriteAsync(icerik);

        TempData["Bildirim"] = ok
            ? (hatalar.Count > 0 ? "Kaydedildi, ama: " + string.Join(" ", hatalar) : "Filo kaydedildi.")
            : "Kaydedilemedi. App_Data klasörünün yazma izni olduğundan emin olun.";
        TempData["BildirimTur"] = ok && hatalar.Count == 0 ? "ok" : "hata";

        return RedirectToPage("/Panel/Filo");
    }
}
