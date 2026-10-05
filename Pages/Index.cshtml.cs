using MemetogluWeb.Models;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemetogluWeb.Pages;

/// <summary>
/// Yalnızca içerik gösteren sayfaların ortak modeli (ana sayfa, hizmetler,
/// kurumsal, SSS). Form işleyen tek sayfa İletişim'dir.
/// </summary>
public class IcerikSayfasiModel : PageModel
{
    private readonly ContentService _icerik;

    public IcerikSayfasiModel(ContentService icerik) => _icerik = icerik;

    public SiteContent Icerik { get; private set; } = new();

    public void OnGet() => Icerik = _icerik.Read();
}

public sealed class IndexModel(ContentService icerik) : IcerikSayfasiModel(icerik);
public sealed class HizmetlerSayfaModel(ContentService icerik) : IcerikSayfasiModel(icerik);
public sealed class KurumsalModel(ContentService icerik) : IcerikSayfasiModel(icerik);
public sealed class SssModel(ContentService icerik) : IcerikSayfasiModel(icerik);
