using MemetogluWeb.Models;

namespace MemetogluWeb.Services;

/// <summary>Şablonların ve sayfaların ortak kullandığı küçük yardımcılar.</summary>
public static class Yardimci
{
    /// <summary>
    /// wwwroot klasörünün diskteki yolu. Program.cs açılışta bir kez doldurur;
    /// medya adreslerine sürüm damgası eklemek için gerekir.
    /// </summary>
    public static string? KokKlasor { get; set; }

    /// <summary>
    /// İçerikte "img/logo.png" biçiminde saklanan yolu web adresine çevirir.
    /// Zaten tam adresse (http ile başlıyorsa) olduğu gibi bırakır.
    ///
    /// Sonuna dosyanın son değiştirilme zamanından türeyen "?v=..." ekler.
    /// Böylece görseller 30 gün önbellekte kalabilir ama panelden bir dosya
    /// değiştirildiğinde ziyaretçi yenisini anında görür.
    /// </summary>
    public static string Medya(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return "";
        }
        if (yol.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || yol.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return yol;
        }

        var goreli = yol.TrimStart('/');
        var adres = "/" + goreli;
        var damga = Damga(goreli);
        return damga is null ? adres : adres + "?v=" + damga;
    }

    /// <summary>Dosyanın son değiştirilme zamanından kısa bir sürüm damgası üretir.</summary>
    private static string? Damga(string goreli)
    {
        var kok = KokKlasor;
        if (string.IsNullOrEmpty(kok) || goreli.Length == 0)
        {
            return null;
        }

        try
        {
            var tam = Path.GetFullPath(
                Path.Combine(kok, goreli.Replace('/', Path.DirectorySeparatorChar)));

            // Dizin dışına çıkan yol olursa damga koymadan geç.
            if (!tam.StartsWith(Path.GetFullPath(kok), StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var bilgi = new FileInfo(tam);
            return bilgi.Exists
                ? bilgi.LastWriteTimeUtc.Ticks.ToString("x")
                : null;
        }
        catch
        {
            // Damga bir kolaylık; üretilemezse adres damgasız çalışmaya devam eder.
            return null;
        }
    }

    /// <summary>
    /// WhatsApp bağlantısı üretir. Numara geçersizse null döner ve
    /// WhatsApp butonları hiç gösterilmez.
    /// </summary>
    public static string? WhatsAppUrl(WhatsApp wa)
    {
        if (!wa.Enabled)
        {
            return null;
        }

        var rakamlar = new string(wa.Number.Where(char.IsAsciiDigit).ToArray());
        if (rakamlar.Length < 10)
        {
            return null;
        }

        var url = "https://wa.me/" + rakamlar;
        if (!string.IsNullOrWhiteSpace(wa.Message))
        {
            url += "?text=" + Uri.EscapeDataString(wa.Message);
        }
        return url;
    }

    /// <summary>Telefon numarasını tel: bağlantısı için temizler.</summary>
    public static string TelHref(Phone telefon)
    {
        var ham = string.IsNullOrWhiteSpace(telefon.Dial) ? telefon.Label : telefon.Dial;
        var temiz = new string(ham.Where(c => char.IsAsciiDigit(c) || c == '+').ToArray());
        return "tel:" + temiz;
    }

    /// <summary>Firma adını menüdeki iki satıra böler.</summary>
    public static (string Ilk, string Kalan) AdiBol(string firmaAdi)
    {
        var parcalar = firmaAdi.Split(' ', 2);
        return (parcalar[0], parcalar.Length > 1 ? parcalar[1] : "Taşımacılık");
    }

    /// <summary>Sosyal medya simgelerinin SVG yolları.</summary>
    public static readonly Dictionary<string, (string Ad, string Path)> SosyalIkon = new()
    {
        ["facebook"] = ("Facebook", "M14 8.5h2.5V5.6C16.1 5.5 15.2 5.4 14.2 5.4c-2.1 0-3.5 1.3-3.5 3.6V11H8v3h2.7v7.6h3.2V14h2.6l.4-3h-3v-1.6c0-.9.2-1.5 1.1-1.5z"),
        ["instagram"] = ("Instagram", "M12 7.4a4.6 4.6 0 1 0 0 9.2 4.6 4.6 0 0 0 0-9.2zm0 7.6a3 3 0 1 1 0-6 3 3 0 0 1 0 6zm5.8-7.8a1.07 1.07 0 1 1-2.15 0 1.07 1.07 0 0 1 2.15 0zM21 8.8c-.05-1.4-.37-2.65-1.4-3.67C18.58 4.1 17.33 3.78 15.93 3.7 14.5 3.62 9.5 3.62 8.07 3.7c-1.4.08-2.64.4-3.67 1.42C3.38 6.15 3.06 7.4 2.98 8.8c-.08 1.44-.08 6.43 0 7.87.08 1.4.4 2.64 1.42 3.67 1.03 1.02 2.27 1.34 3.67 1.42 1.44.08 6.43.08 7.87 0 1.4-.08 2.65-.4 3.67-1.42 1.02-1.03 1.34-2.27 1.4-3.67.09-1.44.09-6.42 0-7.86zm-1.9 9.4c-.3.77-.9 1.36-1.67 1.67-1.16.46-3.9.35-5.18.35s-4.03.1-5.18-.35a2.98 2.98 0 0 1-1.68-1.67c-.46-1.16-.35-3.9-.35-5.18s-.1-4.03.35-5.18A2.98 2.98 0 0 1 7.07 6.1c1.15-.46 3.9-.35 5.18-.35s4.02-.1 5.18.35c.77.3 1.36.9 1.67 1.67.46 1.15.35 3.9.35 5.18s.11 4.02-.35 5.18z"),
        ["linkedin"] = ("LinkedIn", "M6.94 20H3.56V8.96h3.38V20zM5.25 7.46a1.96 1.96 0 1 1 0-3.92 1.96 1.96 0 0 1 0 3.92zM20.44 20h-3.37v-5.37c0-1.28-.03-2.93-1.79-2.93-1.79 0-2.06 1.4-2.06 2.84V20H9.85V8.96h3.24v1.51h.04a3.55 3.55 0 0 1 3.2-1.76c3.42 0 4.05 2.25 4.05 5.18V20z"),
        ["x"] = ("X", "M17.53 4h2.9l-6.34 7.25L21.5 20h-5.6l-4.38-5.73L6.5 20H3.6l6.78-7.75L3 4h5.75l3.96 5.24L17.53 4zm-1.02 14.27h1.6L8.1 5.64H6.38l10.13 12.63z"),
        ["youtube"] = ("YouTube", "M21.58 7.19a2.5 2.5 0 0 0-1.76-1.77C18.25 5 12 5 12 5s-6.25 0-7.82.42A2.5 2.5 0 0 0 2.42 7.2 26.1 26.1 0 0 0 2 12a26.1 26.1 0 0 0 .42 4.81 2.5 2.5 0 0 0 1.76 1.77C5.75 19 12 19 12 19s6.25 0 7.82-.42a2.5 2.5 0 0 0 1.76-1.77A26.1 26.1 0 0 0 22 12a26.1 26.1 0 0 0-.42-4.81zM10 15.02V8.98L15.2 12 10 15.02z"),
    };

    /// <summary>WhatsApp simgesinin SVG yolu.</summary>
    public const string WhatsAppIkon = "M17.47 14.38c-.3-.15-1.75-.86-2.02-.96-.27-.1-.47-.15-.67.15-.2.3-.76.96-.94 1.16-.17.2-.35.22-.64.07-.3-.15-1.25-.46-2.38-1.47-.88-.78-1.47-1.75-1.64-2.05-.17-.3-.02-.46.13-.6.13-.14.3-.35.45-.53.15-.17.2-.3.3-.5.1-.2.05-.37-.02-.52-.08-.15-.67-1.6-.92-2.2-.24-.58-.49-.5-.67-.51h-.57c-.2 0-.52.07-.8.37-.27.3-1.04 1.02-1.04 2.48s1.07 2.88 1.22 3.08c.15.2 2.1 3.2 5.08 4.49.71.3 1.26.49 1.7.63.71.22 1.36.19 1.87.12.57-.09 1.75-.72 2-1.41.25-.7.25-1.29.17-1.41-.07-.13-.27-.2-.57-.35zM12.04 21.5h-.01a9.4 9.4 0 0 1-4.79-1.31l-.34-.2-3.56.93.95-3.47-.22-.36a9.38 9.38 0 0 1-1.44-5 9.42 9.42 0 0 1 16.09-6.66 9.36 9.36 0 0 1 2.76 6.67c0 5.18-4.23 9.4-9.44 9.4zM20.52 3.57A11.36 11.36 0 0 0 12.04.06C5.8.06.72 5.13.72 11.37c0 2 .52 3.94 1.52 5.66L.63 23.94l7.05-1.85a11.3 11.3 0 0 0 5.4 1.38h.01c6.24 0 11.32-5.08 11.32-11.32a11.25 11.25 0 0 0-3.32-8.02z";
}
