using System.ComponentModel.DataAnnotations;
using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages;

public sealed class IndexModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly MailService _posta;
    private readonly ILogger<IndexModel> _log;

    public IndexModel(ContentService icerik, MailService posta, ILogger<IndexModel> log)
    {
        _icerik = icerik;
        _posta = posta;
        _log = log;
    }

    public SiteContent Icerik { get; private set; } = new();

    [BindProperty]
    public TeklifFormu Form { get; set; } = new();

    /// <summary>Form gönderildikten sonra kullanıcıya gösterilecek mesaj.</summary>
    public string? SonucMetni { get; private set; }

    public bool SonucBasarili { get; private set; }

    public void OnGet()
    {
        Icerik = _icerik.Read();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Icerik = _icerik.Read();

        // Bot tuzağı: gerçek ziyaretçi bu alanı göremez, doldurulmuşsa
        // sessizce "teşekkürler" deyip hiçbir şey göndermeyiz.
        if (!string.IsNullOrWhiteSpace(Form.Website))
        {
            SonucBasarili = true;
            SonucMetni = "Talebiniz alındı.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            SonucBasarili = false;
            SonucMetni = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Lütfen zorunlu alanları doldurun.";
            return Page();
        }

        var satirlar = new List<(string, string)>
        {
            ("Ad soyad", Form.Ad),
            ("Telefon", Form.Telefon),
            ("E-posta", Bos(Form.Eposta)),
            ("Nereden", Bos(Form.Nereden)),
            ("Nereye", Bos(Form.Nereye)),
            ("Hizmet türü", Bos(Form.Hizmet)),
            ("Planlanan tarih", Bos(Form.Tarih)),
            ("Yük detayı", Bos(Form.Detay)),
        };

        var duz = string.Join('\n', satirlar.Select(s => $"{s.Item1}: {s.Item2}"));
        var html = MailService.TabloyaCevir(satirlar);

        var (ok, hata) = await _posta.SendAsync(
            $"Teklif talebi — {Form.Ad} ({Form.Telefon})",
            html, duz, Form.Eposta);

        if (ok)
        {
            SonucBasarili = true;
            SonucMetni = $"Teşekkürler {Form.Ad}. Talebiniz bize ulaştı, en kısa sürede dönüş yapacağız.";
            Form = new TeklifFormu(); // formu temizle
            ModelState.Clear();
            return Page();
        }

        // E-posta gitmese bile talebi kaybetmeyelim.
        _log.LogWarning("Teklif e-postası gönderilemedi: {Hata}", hata);
        await _icerik.LogQuoteAsync($"{DateTime.Now:O} | {duz.Replace('\n', '|')}");

        SonucBasarili = false;
        SonucMetni = "Talebiniz gönderilemedi. Lütfen telefonla arayın ya da WhatsApp’tan yazın — hemen dönüş yapalım.";
        return Page();

        static string Bos(string? d) => string.IsNullOrWhiteSpace(d) ? "—" : d.Trim();
    }
}

/// <summary>Teklif formunun alanları ve doğrulama kuralları.</summary>
public sealed class TeklifFormu
{
    [Required(ErrorMessage = "Lütfen ad soyad alanını doldurun.")]
    [StringLength(120, ErrorMessage = "Ad soyad çok uzun.")]
    public string Ad { get; set; } = "";

    [Required(ErrorMessage = "Lütfen telefon alanını doldurun.")]
    [StringLength(40)]
    [RegularExpression(@"^[\d\s\+\(\)\-]{10,}$",
        ErrorMessage = "Telefon numarası eksik görünüyor. Lütfen kontrol edin.")]
    public string Telefon { get; set; } = "";

    [EmailAddress(ErrorMessage = "E-posta adresi geçerli değil.")]
    [StringLength(160)]
    public string? Eposta { get; set; }

    [StringLength(120)] public string? Nereden { get; set; }
    [StringLength(120)] public string? Nereye { get; set; }
    [StringLength(120)] public string? Hizmet { get; set; }
    [StringLength(40)] public string? Tarih { get; set; }
    [StringLength(2000)] public string? Detay { get; set; }

    /// <summary>Bot tuzağı — ekranda gizlidir, dolu gelirse istek yok sayılır.</summary>
    public string? Website { get; set; }
}
