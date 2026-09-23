using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>
/// İlk kurulum. Yalnızca App_Data/config.json yokken çalışır;
/// kurulum tamamlandıktan sonra bu sayfa kendini kilitler.
/// </summary>
public sealed class KurulumModel : PageModel
{
    private readonly ContentService _icerik;

    public KurulumModel(ContentService icerik) => _icerik = icerik;

    public bool ZatenKurulu { get; private set; }
    public List<string> Hatalar { get; } = new();

    [BindProperty] public string Eposta { get; set; } = "info@memetoglutasimacilik.com";
    [BindProperty] public string Sifre { get; set; } = "";
    [BindProperty] public string Sifre2 { get; set; } = "";
    [BindProperty] public string MailTo { get; set; } = "info@memetoglutasimacilik.com";
    [BindProperty] public string MailFrom { get; set; } = "info@memetoglutasimacilik.com";
    [BindProperty] public string SmtpHost { get; set; } = "";
    [BindProperty] public int SmtpPort { get; set; } = 587;
    [BindProperty] public string SmtpUser { get; set; } = "";
    [BindProperty] public string SmtpPass { get; set; } = "";
    [BindProperty] public bool SmtpSsl { get; set; } = true;

    public void OnGet()
    {
        ZatenKurulu = _icerik.ReadConfig().IsConfigured;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (_icerik.ReadConfig().IsConfigured)
        {
            ZatenKurulu = true;
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Eposta) || !Eposta.Contains('@'))
        {
            Hatalar.Add("Geçerli bir yönetici e-posta adresi girin.");
        }
        if ((Sifre ?? "").Length < 10)
        {
            Hatalar.Add("Şifre en az 10 karakter olmalı.");
        }
        if (Sifre != Sifre2)
        {
            Hatalar.Add("Şifreler birbiriyle eşleşmiyor.");
        }
        if (string.IsNullOrWhiteSpace(MailTo) || !MailTo.Contains('@'))
        {
            Hatalar.Add("Formun düşeceği e-posta adresi geçerli değil.");
        }

        if (Hatalar.Count > 0)
        {
            return Page();
        }

        var ayar = new AdminConfig
        {
            AdminEmail = Eposta.Trim(),
            PasswordHash = PasswordHasher.Hash(Sifre!),
            MailMode = string.IsNullOrWhiteSpace(SmtpHost) ? "yok" : "smtp",
            MailTo = MailTo.Trim(),
            MailFrom = string.IsNullOrWhiteSpace(MailFrom) ? MailTo.Trim() : MailFrom.Trim(),
            MailFromName = "Memetoglu Web Sitesi",
            SmtpHost = (SmtpHost ?? "").Trim(),
            SmtpPort = SmtpPort > 0 ? SmtpPort : 587,
            SmtpUser = (SmtpUser ?? "").Trim(),
            SmtpPass = SmtpPass ?? "",
            SmtpSsl = SmtpSsl,
        };

        if (!await _icerik.WriteConfigAsync(ayar))
        {
            Hatalar.Add("config.json yazılamadı. App_Data klasörünün yazma izni olduğundan emin olun.");
            return Page();
        }

        TempData["Bildirim"] = "Kurulum tamamlandı. Şimdi e-posta ve şifrenizle giriş yapın.";
        TempData["BildirimTur"] = "ok";
        return RedirectToPage("/Panel/Giris");
    }
}
