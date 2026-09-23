using System.Security.Claims;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Panel giriş ekranı.</summary>
public sealed class GirisModel : PageModel
{
    private readonly ContentService _icerik;
    private readonly ILogger<GirisModel> _log;

    // Kaba kuvvet denemelerini yavaşlatır: aynı IP'den 5 hatalı giriş → 15 dk bekleme.
    private static readonly Dictionary<string, (int Adet, DateTime Son)> Denemeler = new();
    private static readonly object Kilit = new();
    private const int MaksDeneme = 5;
    private static readonly TimeSpan BeklemeSuresi = TimeSpan.FromMinutes(15);

    public GirisModel(ContentService icerik, ILogger<GirisModel> log)
    {
        _icerik = icerik;
        _log = log;
    }

    [BindProperty] public string Eposta { get; set; } = "";
    [BindProperty] public string Sifre { get; set; } = "";

    public string? Hata { get; private set; }

    public IActionResult OnGet()
    {
        // Kurulum yapılmadıysa önce onu tamamlat.
        if (!_icerik.ReadConfig().IsConfigured)
        {
            return RedirectToPage("/Panel/Kurulum");
        }
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Panel/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var cfg = _icerik.ReadConfig();
        if (!cfg.IsConfigured)
        {
            return RedirectToPage("/Panel/Kurulum");
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "bilinmiyor";

        var kalan = KalanBekleme(ip);
        if (kalan > TimeSpan.Zero)
        {
            Hata = $"Çok fazla hatalı deneme. {Math.Ceiling(kalan.TotalMinutes)} dakika sonra tekrar deneyin.";
            return Page();
        }

        var epostaDogru = string.Equals(cfg.AdminEmail, Eposta?.Trim(), StringComparison.OrdinalIgnoreCase);
        var sifreDogru = PasswordHasher.Verify(Sifre ?? "", cfg.PasswordHash);

        if (epostaDogru && sifreDogru)
        {
            DenemeSifirla(ip);

            var kimlik = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, cfg.AdminEmail),
                    new Claim(ClaimTypes.Role, "Yonetici"),
                },
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(kimlik),
                new AuthenticationProperties { IsPersistent = false });

            return RedirectToPage("/Panel/Index");
        }

        DenemeKaydet(ip);
        _log.LogWarning("Başarısız panel girişi. IP: {Ip}", ip);

        // Hangisinin yanlış olduğunu söylemiyoruz: hesap taramasını zorlaştırır.
        Hata = "E-posta veya şifre hatalı.";
        return Page();
    }

    private static TimeSpan KalanBekleme(string ip)
    {
        lock (Kilit)
        {
            if (!Denemeler.TryGetValue(ip, out var kayit) || kayit.Adet < MaksDeneme)
            {
                return TimeSpan.Zero;
            }
            var gecen = DateTime.UtcNow - kayit.Son;
            return gecen >= BeklemeSuresi ? TimeSpan.Zero : BeklemeSuresi - gecen;
        }
    }

    private static void DenemeKaydet(string ip)
    {
        lock (Kilit)
        {
            var adet = Denemeler.TryGetValue(ip, out var k) ? k.Adet : 0;
            Denemeler[ip] = (adet + 1, DateTime.UtcNow);

            // Bir günden eski kayıtları temizle.
            var eskiler = Denemeler
                .Where(x => DateTime.UtcNow - x.Value.Son > TimeSpan.FromDays(1))
                .Select(x => x.Key)
                .ToList();
            foreach (var e in eskiler) { Denemeler.Remove(e); }
        }
    }

    private static void DenemeSifirla(string ip)
    {
        lock (Kilit) { Denemeler.Remove(ip); }
    }
}
