namespace MemetogluWeb.Models;

/// <summary>
/// Panel formlarındaki tek bir alanın görünüm modeli.
/// Pages/Panel/_Alan.cshtml bunu kullanarak etiketi, girdiyi ve ipucunu basar.
/// </summary>
public sealed class AlanVm
{
    public required string Ad { get; init; }
    public required string Etiket { get; init; }
    public string? Deger { get; init; }
    public string? Ipucu { get; init; }

    /// <summary>metin | uzun | kutu | gorsel | sayi</summary>
    public string Tur { get; init; } = "metin";

    public bool Isaretli { get; init; }
    public int Satir { get; init; } = 3;
    public string? YerTutucu { get; init; }

    public string Id => "f_" + Ad.Replace('[', '_').Replace(']', '_').Replace('.', '_');
}
