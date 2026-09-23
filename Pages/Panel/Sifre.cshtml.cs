using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Şifre değiştirme ve e-posta gönderim ayarları.</summary>
public sealed class SifreModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly MailService _posta;

    public SifreModel(ContentService icerik, MailService posta)
    {
        _icerik = icerik;
        _posta = posta;
    }

    public Models.AdminConfig Ayar { get; private set; } = new();
    public List<string> Hatalar { get; } = new();

    public void OnGet() => Ayar = _icerik.ReadConfig();

    // ---------------------------------------------------------- Şifre

    public async Task<IActionResult> OnPostSifreAsync(string eski, string yeni, string yeni2)
    {
        var cfg = _icerik.ReadConfig();
        Ayar = cfg;

        if (!PasswordHasher.Verify(eski ?? "", cfg.PasswordHash))
        {
            Hatalar.Add("Mevcut şifre hatalı.");
        }
        if ((yeni ?? "").Length < 10)
        {
            Hatalar.Add("Yeni şifre en az 10 karakter olmalı.");
        }
        if (yeni != yeni2)
        {
            Hatalar.Add("Yeni şifreler birbiriyle eşleşmiyor.");
        }

        if (Hatalar.Count > 0)
        {
            return Page();
        }

        cfg.PasswordHash = PasswordHasher.Hash(yeni!);

        if (!await _icerik.WriteConfigAsync(cfg))
        {
            Hatalar.Add("config.json yazılamadı. App_Data klasörünün yazma izni olmalı.");
            return Page();
        }

        TempData["Bildirim"] = "Şifreniz değiştirildi.";
        TempData["BildirimTur"] = "ok";
        return RedirectToPage("/Panel/Sifre");
    }

    // ---------------------------------------------------------- E-posta

    public async Task<IActionResult> OnPostEpostaAsync(
        string mailTo, string mailFrom, string smtpHost, int smtpPort,
        string smtpUser, string? smtpPass, bool smtpSsl)
    {
        var cfg = _icerik.ReadConfig();
        Ayar = cfg;

        cfg.MailTo = (mailTo ?? "").Trim();
        cfg.MailFrom = (mailFrom ?? "").Trim();
        cfg.SmtpHost = (smtpHost ?? "").Trim();
        cfg.SmtpPort = smtpPort > 0 ? smtpPort : 587;
        cfg.SmtpUser = (smtpUser ?? "").Trim();
        cfg.SmtpSsl = smtpSsl;
        cfg.MailMode = string.IsNullOrWhiteSpace(cfg.SmtpHost) ? "yok" : "smtp";

        // Şifre alanı boş bırakılırsa mevcut şifre korunur.
        if (!string.IsNullOrEmpty(smtpPass))
        {
            cfg.SmtpPass = smtpPass;
        }

        if (!cfg.MailTo.Contains('@'))
        {
            Hatalar.Add("Talebin düşeceği e-posta adresi geçerli değil.");
        }
        if (!cfg.MailFrom.Contains('@'))
        {
            Hatalar.Add("Gönderen e-posta adresi geçerli değil.");
        }

        if (Hatalar.Count > 0)
        {
            return Page();
        }

        if (!await _icerik.WriteConfigAsync(cfg))
        {
            Hatalar.Add("config.json yazılamadı.");
            return Page();
        }

        TempData["Bildirim"] = "E-posta ayarları kaydedildi.";
        TempData["BildirimTur"] = "ok";
        return RedirectToPage("/Panel/Sifre");
    }

    // ---------------------------------------------------------- Test

    public async Task<IActionResult> OnPostTestAsync()
    {
        var (ok, hata) = await _posta.SendAsync(
            "Test — Memetoğlu web sitesi",
            "<p>Bu bir test mesajıdır. Bu postayı aldıysanız teklif formu çalışıyor demektir.</p>",
            "Bu bir test mesajıdır. Bu postayı aldıysanız teklif formu çalışıyor demektir.");

        TempData["Bildirim"] = ok
            ? "Test postası gönderildi. Gelen kutunuzu (ve spam klasörünü) kontrol edin."
            : $"Test postası gönderilemedi. {hata}";
        TempData["BildirimTur"] = ok ? "ok" : "hata";

        return RedirectToPage("/Panel/Sifre");
    }
}
