using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages.Panel;

/// <summary>Oturumu kapatır. Yalnızca POST ile çalışır (CSRF koruması).</summary>
public sealed class CikisModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Panel/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Panel/Giris");
    }
}
