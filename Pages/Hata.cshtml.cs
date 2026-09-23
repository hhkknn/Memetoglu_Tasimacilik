using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages;

/// <summary>
/// Beklenmeyen hatalarda gösterilen sayfa.
/// Ziyaretçiye teknik ayrıntı göstermez; ayrıntı sunucu günlüğüne yazılır.
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public sealed class HataModel : PageModel
{
    /// <summary>Günlükteki kaydı bulmayı kolaylaştıran istek kimliği.</summary>
    public string? IstekKodu { get; private set; }

    public void OnGet()
    {
        IstekKodu = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}
