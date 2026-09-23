using System.Text.Json;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Yüklenen görsellerin listesi; yükleme ve silme.</summary>
public sealed class MedyaModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly UploadService _yukleme;

    public MedyaModel(ContentService icerik, UploadService yukleme)
    {
        _icerik = icerik;
        _yukleme = yukleme;
    }

    public IReadOnlyList<FileInfo> Dosyalar { get; private set; } = Array.Empty<FileInfo>();

    /// <summary>İçerikte kullanılan dosya adları — kullanımdakiler silinemez.</summary>
    public HashSet<string> Kullanimda { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public void OnGet() => Yukle();

    public async Task<IActionResult> OnPostYukleAsync(List<IFormFile>? dosyalar)
    {
        var basarili = 0;
        var hatalar = new List<string>();

        foreach (var dosya in dosyalar ?? new List<IFormFile>())
        {
            var (yol, hata) = await _yukleme.SaveImageAsync(dosya);
            if (yol is not null) { basarili++; }
            if (hata is not null) { hatalar.Add($"{dosya.FileName}: {hata}"); }
        }

        if (basarili > 0 && hatalar.Count == 0)
        {
            TempData["Bildirim"] = $"{basarili} görsel yüklendi. Artık ilgili bölümlerden seçebilirsiniz.";
            TempData["BildirimTur"] = "ok";
        }
        else if (hatalar.Count > 0)
        {
            TempData["Bildirim"] = string.Join(" ", hatalar);
            TempData["BildirimTur"] = "hata";
        }

        return RedirectToPage("/Panel/Medya");
    }

    public IActionResult OnPostSil(string dosya)
    {
        Yukle();

        var ad = Path.GetFileName(dosya ?? "");
        if (Kullanimda.Contains(ad))
        {
            TempData["Bildirim"] = "Bu görsel sitede kullanılıyor. Önce kullanıldığı bölümden değiştirin.";
            TempData["BildirimTur"] = "hata";
        }
        else if (_yukleme.DeleteUpload(ad))
        {
            TempData["Bildirim"] = "Görsel silindi.";
            TempData["BildirimTur"] = "ok";
        }
        else
        {
            TempData["Bildirim"] = "Görsel silinemedi. Dosya izinlerini kontrol edin.";
            TempData["BildirimTur"] = "hata";
        }

        return RedirectToPage("/Panel/Medya");
    }

    private void Yukle()
    {
        Dosyalar = _yukleme.ListUploads();

        // İçeriğin tamamını metne çevirip hangi dosya adlarının geçtiğine bakarız.
        var json = JsonSerializer.Serialize(_icerik.Read());
        Kullanimda = Dosyalar
            .Where(d => json.Contains(d.Name, StringComparison.OrdinalIgnoreCase))
            .Select(d => d.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
