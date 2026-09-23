using System.Buffers.Binary;

namespace MemetogluWeb.Services;

/// <summary>
/// Panelden yüklenen görsel ve videoları doğrular ve wwwroot altına kaydeder.
///
/// Güvenlik yaklaşımı: dosya adına ve uzantısına GÜVENİLMEZ. Dosyanın ilk
/// baytlarına (imza/"magic number") bakılarak gerçek türü belirlenir, dosya
/// adı tamamen yeniden üretilir.
/// </summary>
public sealed class UploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UploadService> _log;

    public const long MaxImageBytes = 6 * 1024 * 1024;   // 6 MB
    public const long MaxVideoBytes = 40 * 1024 * 1024;  // 40 MB

    public UploadService(IWebHostEnvironment env, ILogger<UploadService> log)
    {
        _env = env;
        _log = log;
    }

    /// <summary>
    /// Görseli kaydeder. Başarılıysa "uploads/ad.jpg" biçiminde web yolu döner;
    /// dosya seçilmemişse null, geçersizse hata mesajı döner.
    /// </summary>
    public async Task<(string? Yol, string? Hata)> SaveImageAsync(IFormFile? dosya)
    {
        if (dosya is null || dosya.Length == 0)
        {
            return (null, null); // dosya seçilmemiş — hata değil
        }
        if (dosya.Length > MaxImageBytes)
        {
            return (null, "Görsel 6 MB sınırını aşıyor. Lütfen küçültüp tekrar deneyin.");
        }

        var uzanti = await GorselTuruBulAsync(dosya);
        if (uzanti is null)
        {
            return (null, "Bu dosya geçerli bir görsel değil. JPG, PNG, WEBP veya GIF yükleyin.");
        }

        return await KaydetAsync(dosya, "uploads", uzanti);
    }

    /// <summary>Videoyu kaydeder. Yalnızca MP4 ve WEBM kabul edilir.</summary>
    public async Task<(string? Yol, string? Hata)> SaveVideoAsync(IFormFile? dosya, string onEk = "hero")
    {
        if (dosya is null || dosya.Length == 0)
        {
            return (null, null);
        }
        if (dosya.Length > MaxVideoBytes)
        {
            return (null, "Video 40 MB sınırını aşıyor. Hero videosu için 5 MB altı önerilir.");
        }

        var uzanti = await VideoTuruBulAsync(dosya);
        if (uzanti is null)
        {
            return (null, "Yalnızca MP4 veya WEBM video yükleyebilirsiniz.");
        }

        return await KaydetAsync(dosya, "video", uzanti, onEk);
    }

    // ------------------------------------------------------------ Kaydetme

    private async Task<(string? Yol, string? Hata)> KaydetAsync(
        IFormFile dosya, string altKlasor, string uzanti, string? onEk = null)
    {
        try
        {
            var klasor = Path.Combine(_env.WebRootPath, altKlasor);
            Directory.CreateDirectory(klasor);

            // Dosya adını biz üretiriz; kullanıcıdan gelen ad kullanılmaz.
            var temiz = onEk ?? TemizAd(Path.GetFileNameWithoutExtension(dosya.FileName));
            var benzersiz = Guid.NewGuid().ToString("N")[..8];
            var ad = $"{temiz}-{benzersiz}.{uzanti}";
            var tamYol = Path.Combine(klasor, ad);

            await using (var akis = new FileStream(tamYol, FileMode.Create, FileAccess.Write))
            {
                await dosya.CopyToAsync(akis);
            }

            return ($"{altKlasor}/{ad}", null);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Dosya kaydedilemedi: {Klasor}", altKlasor);
            return (null, "Dosya kaydedilemedi. Klasörün yazma izni olduğundan emin olun.");
        }
    }

    /// <summary>Dosya adını güvenli hâle getirir: yalnızca harf, rakam ve tire.</summary>
    private static string TemizAd(string ham)
    {
        var karakterler = ham.ToLowerInvariant()
            .Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-')
            .ToArray();

        var ad = new string(karakterler).Trim('-');
        while (ad.Contains("--"))
        {
            ad = ad.Replace("--", "-");
        }

        if (string.IsNullOrWhiteSpace(ad))
        {
            ad = "gorsel";
        }
        return ad.Length > 32 ? ad[..32] : ad;
    }

    // ------------------------------------------------------------ Tür tespiti

    /// <summary>
    /// Dosyanın ilk baytlarına bakarak gerçek görsel türünü bulur.
    /// Uzantı değiştirilerek betik yüklenmesini engeller.
    /// </summary>
    private static async Task<string?> GorselTuruBulAsync(IFormFile dosya)
    {
        var bas = await IlkBaytlarAsync(dosya, 16);
        if (bas.Length < 12)
        {
            return null;
        }

        // JPEG: FF D8 FF
        if (bas[0] == 0xFF && bas[1] == 0xD8 && bas[2] == 0xFF)
        {
            return "jpg";
        }
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (bas[0] == 0x89 && bas[1] == 0x50 && bas[2] == 0x4E && bas[3] == 0x47
            && bas[4] == 0x0D && bas[5] == 0x0A && bas[6] == 0x1A && bas[7] == 0x0A)
        {
            return "png";
        }
        // GIF: "GIF8"
        if (bas[0] == 'G' && bas[1] == 'I' && bas[2] == 'F' && bas[3] == '8')
        {
            return "gif";
        }
        // WEBP: "RIFF" .... "WEBP"
        if (bas[0] == 'R' && bas[1] == 'I' && bas[2] == 'F' && bas[3] == 'F'
            && bas[8] == 'W' && bas[9] == 'E' && bas[10] == 'B' && bas[11] == 'P')
        {
            return "webp";
        }

        return null;
    }

    /// <summary>Video türünü ilk baytlardan bulur.</summary>
    private static async Task<string?> VideoTuruBulAsync(IFormFile dosya)
    {
        var bas = await IlkBaytlarAsync(dosya, 16);
        if (bas.Length < 12)
        {
            return null;
        }

        // WEBM/Matroska: 1A 45 DF A3
        if (bas[0] == 0x1A && bas[1] == 0x45 && bas[2] == 0xDF && bas[3] == 0xA3)
        {
            return "webm";
        }

        // MP4: 4-8. baytlar "ftyp"
        if (bas[4] == 'f' && bas[5] == 't' && bas[6] == 'y' && bas[7] == 'p')
        {
            // Kutu uzunluğu makul mü diye bakalım (bozuk dosyaları eler).
            var uzunluk = BinaryPrimitives.ReadUInt32BigEndian(bas.AsSpan(0, 4));
            if (uzunluk is >= 8 and <= 1024)
            {
                return "mp4";
            }
        }

        return null;
    }

    private static async Task<byte[]> IlkBaytlarAsync(IFormFile dosya, int adet)
    {
        await using var akis = dosya.OpenReadStream();
        var tampon = new byte[adet];
        var okunan = await akis.ReadAsync(tampon.AsMemory(0, adet));
        return okunan < adet ? tampon[..okunan] : tampon;
    }

    // ------------------------------------------------------------ Listeleme

    /// <summary>uploads klasöründeki dosyaları yeniden eskiye sıralar.</summary>
    public IReadOnlyList<FileInfo> ListUploads()
    {
        var klasor = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(klasor))
        {
            return Array.Empty<FileInfo>();
        }

        return new DirectoryInfo(klasor)
            .GetFiles()
            .Where(f => !f.Name.StartsWith('.'))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();
    }

    /// <summary>
    /// Yüklenen bir dosyayı siler. Yol dolaşımına karşı korumalıdır:
    /// yalnızca uploads klasörünün içindeki dosyalar silinebilir.
    /// </summary>
    public bool DeleteUpload(string dosyaAdi)
    {
        try
        {
            var klasor = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads"));
            var hedef = Path.GetFullPath(Path.Combine(klasor, Path.GetFileName(dosyaAdi)));

            if (!hedef.StartsWith(klasor, StringComparison.Ordinal) || !File.Exists(hedef))
            {
                return false;
            }

            File.Delete(hedef);
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Dosya silinemedi: {Ad}", dosyaAdi);
            return false;
        }
    }
}
