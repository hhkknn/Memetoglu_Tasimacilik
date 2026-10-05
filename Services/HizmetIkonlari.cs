namespace MemetogluWeb.Services;

/// <summary>
/// Hizmet kartlarında fotoğraf yerine gösterilebilen çizgi ikonlar.
/// İçerikte yalnızca anahtar saklanır ("koli", "aski"…); SVG buradan basılır.
/// Yeni ikon eklemek için listeye bir satır eklemek yeterlidir; panel seçeneği otomatik çıkar.
/// </summary>
public static class HizmetIkonlari
{
    public static readonly IReadOnlyList<(string Anahtar, string Ad, string Svg)> Liste = new[]
    {
        ("koli", "Koliler",
            """<path d="M8 26l16-7 16 7v18l-16 7-16-7z"/><path d="M8 26l16 7 16-7M24 33v18"/><g opacity=".55"><path d="M30 14l14-6 14 6v16l-14 6"/><path d="M30 14l14 6 14-6M44 20v6"/></g>"""),
        ("dedike", "Kamyonet ve onay",
            """<path d="M4 42V22h30v20M34 28h12l8 8v6h-4"/><circle cx="16" cy="44" r="4"/><circle cx="44" cy="44" r="4"/><path d="M20 44h20M4 42h8"/><circle cx="50" cy="16" r="9"/><path d="M46 16l3 3 5-6"/>"""),
        ("aski", "Elbise askısı",
            """<path d="M32 22v-3a5 5 0 1 1 5 5"/><path d="M32 24L8 40a3 3 0 0 0 2 5h44a3 3 0 0 0 2-5z"/><path d="M14 52h36" opacity=".55"/>"""),
        ("sehir", "Şehir binaları",
            """<path d="M6 54h52M10 54V22h14v32M24 54V12h16v42M40 54V28h14v26"/><path d="M15 28h4M15 36h4M15 44h4M29 18h6M29 26h6M29 34h6M45 34h4M45 42h4"/>"""),
        ("rota", "Rota ve konum",
            """<circle cx="14" cy="14" r="5"/><path d="M14 19c0 6 4 8 10 8h16c6 0 10 3 10 9s-4 9-10 9H22" stroke-dasharray="3 4"/><path d="M22 42c0 7-6 14-6 14s-6-7-6-14a6 6 0 0 1 12 0z"/><circle cx="16" cy="42" r="2"/>"""),
        ("kamyon", "Dolu kamyon",
            """<path d="M4 44V16h36v28M40 26h10l10 10v8h-4"/><circle cx="14" cy="46" r="4"/><circle cx="48" cy="46" r="4"/><path d="M18 46h26M4 44h6"/><path d="M10 22h10v8H10zM22 22h12v8H22zM10 32h24v6H10z" opacity=".6"/>"""),
        ("anahtar", "Anahtar",
            """<circle cx="22" cy="32" r="10"/><circle cx="22" cy="32" r="3"/><path d="M32 32h24M48 32v7M54 32v5"/>"""),
        ("palet", "Palet ve forklift",
            """<path d="M6 50h30M8 50v-4h26v4M12 46V30h18v16"/><path d="M12 38h18" opacity=".55"/><path d="M42 50V18M42 26h10l4 10v14h-14"/><circle cx="48" cy="52" r="3"/>"""),
        ("saat", "Saat / zamanında teslim",
            """<circle cx="32" cy="34" r="20"/><path d="M32 22v12l8 6"/><path d="M26 8h12M32 8v6"/>"""),
    };

    public static string? Svg(string? anahtar) =>
        Liste.FirstOrDefault(i => i.Anahtar == anahtar).Svg;
}
