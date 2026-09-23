using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Süreç adımlarının düzenlendiği ekran.</summary>
public sealed class SurecModel : PageModel
{
    private readonly ContentService _icerik;

    public SurecModel(ContentService icerik) => _icerik = icerik;

    public SiteContent Icerik { get; private set; } = new();

    public void OnGet() => Icerik = _icerik.Read();

    public async Task<IActionResult> OnPostAsync()
    {
        var icerik = _icerik.Read();
        var f = Request.Form;

        var basliklar = f["baslik"];
        var aciklamalar = f["aciklama"];
        var yeni = new List<ProcessStep>();

        for (var i = 0; i < basliklar.Count; i++)
        {
            var b = (basliklar[i] ?? "").Trim();
            var a = (i < aciklamalar.Count ? aciklamalar[i] : null)?.Trim() ?? "";
            if (b.Length == 0 && a.Length == 0)
            {
                continue;
            }
            yeni.Add(new ProcessStep { Title = b, Description = a });
        }

        icerik.Process = yeni;
        var ok = await _icerik.WriteAsync(icerik);

        TempData["Bildirim"] = ok
            ? "Süreç adımları kaydedildi."
            : "Kaydedilemedi. App_Data klasörünün yazma izni olduğundan emin olun.";
        TempData["BildirimTur"] = ok ? "ok" : "hata";

        return RedirectToPage("/Panel/Surec");
    }
}
