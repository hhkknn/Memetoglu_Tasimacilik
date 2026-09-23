using System.Text.Json.Serialization;

namespace MemetogluWeb.Models;

/// <summary>
/// Sitenin tüm içeriği. App_Data/content.json dosyasıyla birebir eşleşir.
///
/// Veritabanı yoktur: içerik tek bir JSON dosyasında tutulur. Bu, paylaşımlı
/// hosting'e yüklemeyi ve yedek almayı basitleştirir — dosyayı kopyalamak
/// tüm siteyi yedeklemek demektir.
/// </summary>
public sealed class SiteContent
{
    [JsonPropertyName("settings")]
    public Settings Settings { get; set; } = new();

    [JsonPropertyName("hero")]
    public Hero Hero { get; set; } = new();

    [JsonPropertyName("trust")]
    public TrustBlock Trust { get; set; } = new();

    [JsonPropertyName("stats")]
    public StatsBlock Stats { get; set; } = new();

    [JsonPropertyName("references")]
    public ReferencesBlock References { get; set; } = new();

    [JsonPropertyName("servicesSection")]
    public SectionHeading ServicesSection { get; set; } = new();

    [JsonPropertyName("services")]
    public List<ServiceCard> Services { get; set; } = new();

    [JsonPropertyName("solutions")]
    public SolutionsBlock Solutions { get; set; } = new();

    [JsonPropertyName("statement")]
    public StatementBlock Statement { get; set; } = new();

    [JsonPropertyName("fleetSection")]
    public SectionHeading FleetSection { get; set; } = new();

    [JsonPropertyName("fleet")]
    public List<FleetCard> Fleet { get; set; } = new();

    [JsonPropertyName("coverage")]
    public CoverageBlock Coverage { get; set; } = new();

    [JsonPropertyName("processSection")]
    public SectionHeading ProcessSection { get; set; } = new();

    [JsonPropertyName("process")]
    public List<ProcessStep> Process { get; set; } = new();

    [JsonPropertyName("about")]
    public AboutBlock About { get; set; } = new();

    [JsonPropertyName("testimonials")]
    public TestimonialsBlock Testimonials { get; set; } = new();

    [JsonPropertyName("faq")]
    public FaqBlock Faq { get; set; } = new();

    [JsonPropertyName("contact")]
    public ContactBlock Contact { get; set; } = new();
}

// ---------------------------------------------------------------- Ayarlar

public sealed class Settings
{
    [JsonPropertyName("companyName")] public string CompanyName { get; set; } = "Memetoğlu Taşımacılık";
    [JsonPropertyName("slogan")] public string Slogan { get; set; } = "";
    [JsonPropertyName("logo")] public string Logo { get; set; } = "img/logo.png";
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("phones")] public List<Phone> Phones { get; set; } = new();
    [JsonPropertyName("whatsapp")] public WhatsApp WhatsApp { get; set; } = new();
    [JsonPropertyName("address")] public Address Address { get; set; } = new();
    [JsonPropertyName("workingHours")] public List<string> WorkingHours { get; set; } = new();
    [JsonPropertyName("socials")] public List<Social> Socials { get; set; } = new();
    [JsonPropertyName("seo")] public Seo Seo { get; set; } = new();
    [JsonPropertyName("draftNotice")] public Notice DraftNotice { get; set; } = new();
}

public sealed class Phone
{
    /// <summary>Sitede görünen hâli, örn. "0538 612 90 91".</summary>
    [JsonPropertyName("label")] public string Label { get; set; } = "";

    /// <summary>Tıklanınca aranan numara, örn. "+905386129091".</summary>
    [JsonPropertyName("dial")] public string Dial { get; set; } = "";
}

public sealed class WhatsApp
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;

    /// <summary>Ülke kodlu, yalnızca rakam: 905386129091</summary>
    [JsonPropertyName("number")] public string Number { get; set; } = "";

    [JsonPropertyName("message")] public string Message { get; set; } = "";
}

public sealed class Address
{
    [JsonPropertyName("line1")] public string Line1 { get; set; } = "";
    [JsonPropertyName("district")] public string District { get; set; } = "";
    [JsonPropertyName("city")] public string City { get; set; } = "";
    [JsonPropertyName("postalCode")] public string PostalCode { get; set; } = "";
    [JsonPropertyName("mapsUrl")] public string MapsUrl { get; set; } = "";

    [JsonIgnore] public string OneLine => $"{Line1}, {District} / {City}";
}

public sealed class Social
{
    /// <summary>facebook, instagram, linkedin, x, youtube</summary>
    [JsonPropertyName("platform")] public string Platform { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
}

public sealed class Seo
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";

    /// <summary>Sonunda eğik çizgi olmadan, örn. https://www.memetoglutasimacilik.com</summary>
    [JsonPropertyName("siteUrl")] public string SiteUrl { get; set; } = "";

    [JsonPropertyName("shareImage")] public string ShareImage { get; set; } = "";
}

public sealed class Notice
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    [JsonPropertyName("text")] public string Text { get; set; } = "";
}

// ---------------------------------------------------------------- Bölümler

public sealed class Hero
{
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("headlineTop")] public string HeadlineTop { get; set; } = "";
    [JsonPropertyName("headlineBottom")] public string HeadlineBottom { get; set; } = "";
    [JsonPropertyName("subtitle")] public string Subtitle { get; set; } = "";
    [JsonPropertyName("video")] public string Video { get; set; } = "";
    [JsonPropertyName("poster")] public string Poster { get; set; } = "";
    [JsonPropertyName("videoEnabled")] public bool VideoEnabled { get; set; } = true;
    [JsonPropertyName("primaryButton")] public string PrimaryButton { get; set; } = "";
    [JsonPropertyName("highlights")] public List<string> Highlights { get; set; } = new();
}

public sealed class StatsBlock
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("noticeEnabled")] public bool NoticeEnabled { get; set; }
    [JsonPropertyName("noticeText")] public string NoticeText { get; set; } = "";
    [JsonPropertyName("items")] public List<ValueLabel> Items { get; set; } = new();
}

/// <summary>Rakam/etiket ya da üst satır/alt satır taşıyan basit çift.</summary>
public sealed class ValueLabel
{
    [JsonPropertyName("value")] public string Value { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
}

public sealed class SectionHeading
{
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}

public sealed class ServiceCard
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("image")] public string Image { get; set; } = "";
    [JsonPropertyName("alt")] public string Alt { get; set; } = "";
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();
}

public sealed class FleetCard
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("image")] public string Image { get; set; } = "";
    [JsonPropertyName("alt")] public string Alt { get; set; } = "";
    [JsonPropertyName("specs")] public List<Spec> Specs { get; set; } = new();
}

public sealed class Spec
{
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("value")] public string Value { get; set; } = "";
}

public sealed class SolutionsBlock
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("columns")] public List<SolutionColumn> Columns { get; set; } = new();
}

public sealed class SolutionColumn
{
    [JsonPropertyName("heading")] public string Heading { get; set; } = "";
    [JsonPropertyName("links")] public List<SolutionLink> Links { get; set; } = new();
}

public sealed class SolutionLink
{
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("href")] public string Href { get; set; } = "#iletisim";
}

public sealed class StatementBlock
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("quote")] public string Quote { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("buttonLabel")] public string ButtonLabel { get; set; } = "";
    [JsonPropertyName("image")] public string Image { get; set; } = "";
    [JsonPropertyName("videoEnabled")] public bool VideoEnabled { get; set; } = false;
    [JsonPropertyName("video")] public string Video { get; set; } = "";
}

public sealed class CoverageBlock
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("figures")] public List<ValueLabel> Figures { get; set; } = new();
}

public sealed class ProcessStep
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}

public sealed class AboutBlock
{
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("image")] public string Image { get; set; } = "";
    [JsonPropertyName("alt")] public string Alt { get; set; } = "";
    [JsonPropertyName("features")] public List<Feature> Features { get; set; } = new();
    [JsonPropertyName("vision")] public string Vision { get; set; } = "";
}

public sealed class Feature
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}

public sealed class ContactBlock
{
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("formTitle")] public string FormTitle { get; set; } = "";
    [JsonPropertyName("formNote")] public string FormNote { get; set; } = "";
}

// ---------------------------------------------------------------- Güven şeridi

public sealed class TrustBlock
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("noticeEnabled")] public bool NoticeEnabled { get; set; }
    [JsonPropertyName("noticeText")] public string NoticeText { get; set; } = "";
    [JsonPropertyName("badges")] public List<TrustBadge> Badges { get; set; } = new();
}

public sealed class TrustBadge
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("icon")] public string Icon { get; set; } = "shield";
}

// ---------------------------------------------------------------- Yorumlar

public sealed class TestimonialsBlock
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("noticeEnabled")] public bool NoticeEnabled { get; set; }
    [JsonPropertyName("noticeText")] public string NoticeText { get; set; } = "";
    [JsonPropertyName("googleRating")] public string GoogleRating { get; set; } = "";
    [JsonPropertyName("googleReviewCount")] public string GoogleReviewCount { get; set; } = "";
    [JsonPropertyName("googleUrl")] public string GoogleUrl { get; set; } = "";
    [JsonPropertyName("items")] public List<Testimonial> Items { get; set; } = new();
}

public sealed class Testimonial
{
    [JsonPropertyName("quote")] public string Quote { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("role")] public string Role { get; set; } = "";
    [JsonPropertyName("stars")] public int Stars { get; set; } = 5;
}

// ---------------------------------------------------------------- Sık sorulan sorular

public sealed class FaqBlock
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("eyebrow")] public string Eyebrow { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("items")] public List<FaqItem> Items { get; set; } = new();
}

public sealed class FaqItem
{
    [JsonPropertyName("question")] public string Question { get; set; } = "";
    [JsonPropertyName("answer")] public string Answer { get; set; } = "";
}

// ---------------------------------------------------------------- Referans logoları

public sealed class ReferencesBlock
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("noticeEnabled")] public bool NoticeEnabled { get; set; }
    [JsonPropertyName("noticeText")] public string NoticeText { get; set; } = "";
    [JsonPropertyName("items")] public List<ReferenceLogo> Items { get; set; } = new();
}

/// <summary>Logo boşsa sitede firma adını taşıyan yer tutucu kutu gösterilir.</summary>
public sealed class ReferenceLogo
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("logo")] public string Logo { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
}
