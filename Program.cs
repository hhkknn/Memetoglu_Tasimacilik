using System.Globalization;
using MemetogluWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- Servisler

builder.Services.AddRazorPages(secenekler =>
{
    // /Panel altındaki her sayfa giriş ister; kurulum ve giriş ekranları hariç.
    secenekler.Conventions.AuthorizeFolder("/Panel");
    secenekler.Conventions.AllowAnonymousToPage("/Panel/Giris");
    secenekler.Conventions.AllowAnonymousToPage("/Panel/Kurulum");
});

builder.Services.AddSingleton<ContentService>();
builder.Services.AddSingleton<UploadService>();
builder.Services.AddScoped<MailService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(secenekler =>
    {
        secenekler.LoginPath = "/Panel/Giris";
        secenekler.LogoutPath = "/Panel/Cikis";
        secenekler.AccessDeniedPath = "/Panel/Giris";
        secenekler.ExpireTimeSpan = TimeSpan.FromHours(8);
        secenekler.SlidingExpiration = true;
        secenekler.Cookie.Name = "memetoglu_panel";
        secenekler.Cookie.HttpOnly = true;
        secenekler.Cookie.SameSite = SameSiteMode.Lax;
        // Sunucu HTTPS ise çerez yalnızca HTTPS üzerinden gider.
        secenekler.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// Yükleme boyutu: 40 MB video için.
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 45L * 1024 * 1024;
});

// IIS/Plesk arkasında çalışırken gerçek istemci IP'si ve protokolü okunsun.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

// ---------------------------------------------------------------- Kültür

// Tarih ve sayı biçimleri Türkçe olsun.
var kultur = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = kultur;
CultureInfo.DefaultThreadCurrentUICulture = kultur;

// ---------------------------------------------------------------- Boru hattı

// Medya adreslerine sürüm damgası ekleyebilmek için wwwroot yolunu paylaşıyoruz.
Yardimci.KokKlasor = app.Environment.WebRootPath;

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Hata");
    // Tarayıcıya "bu siteyi hep HTTPS ile aç" der. SSL kurulduktan sonra etkisi başlar.
    app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Görsel, video ve stil dosyaları önbelleğe alınsın: site belirgin hızlanır.
        var yol = ctx.File.Name.ToLowerInvariant();
        if (yol.EndsWith(".jpg") || yol.EndsWith(".jpeg") || yol.EndsWith(".png")
            || yol.EndsWith(".webp") || yol.EndsWith(".gif")
            || yol.EndsWith(".mp4") || yol.EndsWith(".webm"))
        {
            ctx.Context.Response.Headers.CacheControl = "public,max-age=2592000"; // 30 gün
        }
        else if (yol.EndsWith(".css") || yol.EndsWith(".js"))
        {
            ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";  // 7 gün
        }
    }
});

// Güvenlik başlıkları.
app.Use(async (ctx, next) =>
{
    var b = ctx.Response.Headers;
    b.XContentTypeOptions = "nosniff";
    b.XFrameOptions = "SAMEORIGIN";
    b["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// robots.txt ve sitemap.xml kod tarafından üretilir (site adresi panelden gelir).
app.MapGet("/robots.txt", (ContentService icerik) =>
{
    var adres = icerik.Read().Settings.Seo.SiteUrl.TrimEnd('/');
    var satirlar = new List<string>
    {
        "User-agent: *",
        "Allow: /",
        "Disallow: /Panel/",
        "Disallow: /panel/",
        ""
    };
    if (!string.IsNullOrWhiteSpace(adres))
    {
        satirlar.Add($"Sitemap: {adres}/sitemap.xml");
    }
    return Results.Text(string.Join('\n', satirlar), "text/plain");
});

app.MapGet("/sitemap.xml", (ContentService icerik, HttpContext ctx) =>
{
    var ayar = icerik.Read().Settings.Seo;
    var kok = string.IsNullOrWhiteSpace(ayar.SiteUrl)
        ? $"{ctx.Request.Scheme}://{ctx.Request.Host}"
        : ayar.SiteUrl.TrimEnd('/');

    var xml = $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
          <url>
            <loc>{kok}/</loc>
            <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>
            <changefreq>monthly</changefreq>
            <priority>1.0</priority>
          </url>
        </urlset>
        """;

    return Results.Content(xml, "application/xml");
});

app.Run();
