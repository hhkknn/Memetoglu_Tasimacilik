using System.Text.Json.Serialization;

namespace MemetogluWeb.Models;

/// <summary>
/// Yönetici hesabı ve e-posta ayarları.
///
/// App_Data/config.json dosyasında tutulur; kurulum ekranı tarafından bir kez
/// üretilir. Bu dosya gizli bilgi içerir — git'e gönderilmez ve web'den
/// erişilemez (App_Data klasörü statik olarak sunulmaz).
/// </summary>
public sealed class AdminConfig
{
    [JsonPropertyName("adminEmail")]
    public string AdminEmail { get; set; } = "";

    /// <summary>
    /// Şifrenin PBKDF2 özeti. Düz şifre hiçbir yerde saklanmaz.
    /// Biçim: "iterasyon.tuzBase64.ozetBase64"
    /// </summary>
    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; } = "";

    /// <summary>"smtp" ya da "yok" (yok = gönderim kapalı, talep dosyaya yazılır).</summary>
    [JsonPropertyName("mailMode")]
    public string MailMode { get; set; } = "smtp";

    [JsonPropertyName("mailTo")] public string MailTo { get; set; } = "";
    [JsonPropertyName("mailFrom")] public string MailFrom { get; set; } = "";
    [JsonPropertyName("mailFromName")] public string MailFromName { get; set; } = "Memetoglu Web Sitesi";

    [JsonPropertyName("smtpHost")] public string SmtpHost { get; set; } = "";
    [JsonPropertyName("smtpPort")] public int SmtpPort { get; set; } = 587;
    [JsonPropertyName("smtpUser")] public string SmtpUser { get; set; } = "";
    [JsonPropertyName("smtpPass")] public string SmtpPass { get; set; } = "";

    /// <summary>true ise STARTTLS/SSL kullanılır.</summary>
    [JsonPropertyName("smtpSsl")] public bool SmtpSsl { get; set; } = true;

    /// <summary>Kurulum tamamlanmış mı?</summary>
    [JsonIgnore]
    public bool IsConfigured => !string.IsNullOrWhiteSpace(PasswordHash);
}
