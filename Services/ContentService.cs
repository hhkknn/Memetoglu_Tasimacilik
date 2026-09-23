using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using MemetogluWeb.Models;

namespace MemetogluWeb.Services;

/// <summary>
/// İçerik ve yapılandırma dosyalarını okur/yazar.
///
/// Yazma işlemi atomiktir: önce geçici dosyaya yazılır, sonra yerine taşınır.
/// Böylece yazma yarıda kesilse bile content.json bozulmaz. Her kayıtta
/// App_Data/yedek klasörüne zaman damgalı bir kopya bırakılır.
/// </summary>
public sealed class ContentService
{
    private readonly string _dataDir;
    private readonly string _contentPath;
    private readonly string _configPath;
    private readonly string _backupDir;
    private readonly ILogger<ContentService> _log;

    // Aynı anda iki isteğin yazmasını engeller.
    private static readonly SemaphoreSlim Kilit = new(1, 1);

    private static readonly JsonSerializerOptions JsonAyar = new()
    {
        WriteIndented = true,
        // Türkçe karakterler ı gibi kaçışlarla değil, olduğu gibi yazılsın.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNameCaseInsensitive = true,
    };

    public ContentService(IWebHostEnvironment env, ILogger<ContentService> log)
    {
        _log = log;
        _dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        _contentPath = Path.Combine(_dataDir, "content.json");
        _configPath = Path.Combine(_dataDir, "config.json");
        _backupDir = Path.Combine(_dataDir, "yedek");
        Directory.CreateDirectory(_dataDir);
    }

    public string DataDirectory => _dataDir;

    // ------------------------------------------------------------ İçerik

    /// <summary>
    /// İçeriği okur. Dosya yoksa ya da bozuksa boş bir model döner ve günlüğe
    /// yazar — site tamamen çökmek yerine eksik görünür.
    /// </summary>
    public SiteContent Read()
    {
        try
        {
            if (!File.Exists(_contentPath))
            {
                _log.LogWarning("content.json bulunamadı: {Yol}", _contentPath);
                return new SiteContent();
            }

            var ham = File.ReadAllText(_contentPath);
            return JsonSerializer.Deserialize<SiteContent>(ham, JsonAyar) ?? new SiteContent();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "content.json okunamadı.");
            return new SiteContent();
        }
    }

    /// <summary>İçeriği kaydeder. Başarılıysa true döner.</summary>
    public async Task<bool> WriteAsync(SiteContent icerik)
    {
        await Kilit.WaitAsync();
        try
        {
            Yedekle();
            var json = JsonSerializer.Serialize(icerik, JsonAyar);
            return await AtomikYazAsync(_contentPath, json);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "content.json yazılamadı.");
            return false;
        }
        finally
        {
            Kilit.Release();
        }
    }

    // ------------------------------------------------------------ Ayarlar

    public AdminConfig ReadConfig()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return new AdminConfig();
            }
            var ham = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<AdminConfig>(ham, JsonAyar) ?? new AdminConfig();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "config.json okunamadı.");
            return new AdminConfig();
        }
    }

    public async Task<bool> WriteConfigAsync(AdminConfig ayar)
    {
        await Kilit.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(ayar, JsonAyar);
            return await AtomikYazAsync(_configPath, json);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "config.json yazılamadı.");
            return false;
        }
        finally
        {
            Kilit.Release();
        }
    }

    // ------------------------------------------------------------ Yardımcı

    /// <summary>
    /// Önce .tmp dosyasına yazar, sonra yerine taşır.
    /// Yazma yarıda kesilirse asıl dosya bozulmamış kalır.
    /// </summary>
    private async Task<bool> AtomikYazAsync(string hedef, string icerik)
    {
        var gecici = hedef + ".tmp";
        try
        {
            await File.WriteAllTextAsync(gecici, icerik);
            File.Move(gecici, hedef, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Dosya yazılamadı: {Yol}. Klasör yazma izinlerini kontrol edin.", hedef);
            if (File.Exists(gecici))
            {
                try { File.Delete(gecici); } catch { /* yoksayılır */ }
            }
            return false;
        }
    }

    /// <summary>Kaydetmeden önceki hâli yedekler; son 20 kopya tutulur.</summary>
    private void Yedekle()
    {
        try
        {
            if (!File.Exists(_contentPath))
            {
                return;
            }
            Directory.CreateDirectory(_backupDir);

            var ad = $"content-{DateTime.Now:yyyyMMdd-HHmmss}.json";
            File.Copy(_contentPath, Path.Combine(_backupDir, ad), overwrite: true);

            var dosyalar = new DirectoryInfo(_backupDir)
                .GetFiles("content-*.json")
                .OrderByDescending(f => f.Name)
                .Skip(20);

            foreach (var eski in dosyalar)
            {
                try { eski.Delete(); } catch { /* yoksayılır */ }
            }
        }
        catch (Exception ex)
        {
            // Yedek alınamaması kaydetmeyi engellememeli.
            _log.LogWarning(ex, "Yedek alınamadı.");
        }
    }

    /// <summary>
    /// E-postası gönderilemeyen teklif taleplerini dosyaya yazar,
    /// böylece hiçbir talep kaybolmaz.
    /// </summary>
    public async Task LogQuoteAsync(string satir)
    {
        try
        {
            var yol = Path.Combine(_dataDir, "talepler.log");
            await File.AppendAllTextAsync(yol, satir + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Talep günlüğe yazılamadı.");
        }
    }
}
