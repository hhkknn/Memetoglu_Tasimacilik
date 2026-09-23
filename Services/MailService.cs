using System.Net;
using System.Net.Mail;
using System.Text;
using MemetogluWeb.Models;

namespace MemetogluWeb.Services;

/// <summary>
/// Teklif formundan gelen talebi e-posta olarak gönderir.
///
/// .NET'in yerleşik SmtpClient sınıfını kullanır — harici paket gerekmez.
/// Ayarlar panelden (App_Data/config.json) gelir.
/// </summary>
public sealed class MailService
{
    private readonly ContentService _icerik;
    private readonly ILogger<MailService> _log;

    public MailService(ContentService icerik, ILogger<MailService> log)
    {
        _icerik = icerik;
        _log = log;
    }

    /// <summary>
    /// Gönderim sonucunu döndürür. Başarısızsa çağıran taraf talebi
    /// dosyaya yazarak kaybolmasını engeller.
    /// </summary>
    public async Task<(bool Ok, string? Hata)> SendAsync(
        string konu, string htmlGovde, string duzGovde, string? yanitAdresi = null)
    {
        var cfg = _icerik.ReadConfig();

        if (!string.Equals(cfg.MailMode, "smtp", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "E-posta gönderimi kapalı (panelden SMTP ayarlarını girin).");
        }
        if (string.IsNullOrWhiteSpace(cfg.SmtpHost) || string.IsNullOrWhiteSpace(cfg.MailTo))
        {
            return (false, "SMTP sunucusu veya alıcı adresi tanımsız.");
        }

        try
        {
            using var mesaj = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(cfg.MailFrom) ? cfg.SmtpUser : cfg.MailFrom,
                    cfg.MailFromName,
                    Encoding.UTF8),
                Subject = konu,
                SubjectEncoding = Encoding.UTF8,
                Body = htmlGovde,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
            };

            mesaj.To.Add(cfg.MailTo);

            // Düz metin alternatifi: HTML göstermeyen istemciler için.
            mesaj.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                duzGovde, Encoding.UTF8, "text/plain"));

            if (!string.IsNullOrWhiteSpace(yanitAdresi) && yanitAdresi.Contains('@'))
            {
                try { mesaj.ReplyToList.Add(new MailAddress(yanitAdresi)); }
                catch (FormatException) { /* geçersiz adres yoksayılır */ }
            }

            using var istemci = new SmtpClient(cfg.SmtpHost, cfg.SmtpPort)
            {
                EnableSsl = cfg.SmtpSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(cfg.SmtpUser, cfg.SmtpPass),
                Timeout = 20_000,
            };

            await istemci.SendMailAsync(mesaj);
            return (true, null);
        }
        catch (SmtpException ex)
        {
            _log.LogError(ex, "SMTP gönderimi başarısız. Sunucu: {Host}:{Port}", cfg.SmtpHost, cfg.SmtpPort);
            return (false, $"SMTP hatası: {ex.StatusCode} — {ex.Message}");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "E-posta gönderilemedi.");
            return (false, ex.Message);
        }
    }

    /// <summary>Teklif talebini okunaklı bir HTML tabloya çevirir.</summary>
    public static string TabloyaCevir(IEnumerable<(string Etiket, string Deger)> satirlar)
    {
        var sb = new StringBuilder();
        sb.Append("<div style=\"font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#141731\">");
        sb.Append("<h2 style=\"margin:0 0 4px;font-size:18px\">Web sitesinden yeni teklif talebi</h2>");
        sb.Append($"<p style=\"margin:0 0 18px;color:#5a6185;font-size:13px\">{WebUtility.HtmlEncode(DateTime.Now.ToString("dd.MM.yyyy HH:mm"))}</p>");
        sb.Append("<table cellpadding=\"8\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;width:100%;max-width:560px\">");

        var i = 0;
        foreach (var (etiket, deger) in satirlar)
        {
            var zemin = i % 2 == 0 ? "#ffffff" : "#f3f5fb";
            sb.Append($"<tr style=\"background:{zemin}\">");
            sb.Append($"<td style=\"border:1px solid #dce0ef;width:150px;color:#5a6185\">{WebUtility.HtmlEncode(etiket)}</td>");
            sb.Append($"<td style=\"border:1px solid #dce0ef;font-weight:600\">{WebUtility.HtmlEncode(deger).Replace("\n", "<br>")}</td>");
            sb.Append("</tr>");
            i++;
        }

        sb.Append("</table></div>");
        return sb.ToString();
    }
}
