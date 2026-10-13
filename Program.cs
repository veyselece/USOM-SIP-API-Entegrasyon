using System.Net;
using SipPortal.Components;
using SipPortal.Services;

var builder = WebApplication.CreateBuilder(args);

// User Secrets yalnızca geliştirme ortamında kullanılır. Üretim ortamında token
// için ayrı bir güvenli sır yönetimi kaynağı yapılandırılmalıdır.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<SipApiOptions>(builder.Configuration.GetSection("SipApi"));
builder.Services.AddSingleton<CredentialStore>();
builder.Services.AddSingleton<ProxySettingsStore>();
builder.Services.AddTransient<DynamicProxyHandler>();

builder.Services.PostConfigure<SipApiOptions>(o =>
{
    var envToken = Environment.GetEnvironmentVariable("SIP_API_TOKEN");
    if (!string.IsNullOrWhiteSpace(envToken)) o.Token = envToken;

    var header = Environment.GetEnvironmentVariable("SIP_API_AUTH_HEADER");
    if (!string.IsNullOrWhiteSpace(header)) o.AuthHeaderName = header;

    var scheme = Environment.GetEnvironmentVariable("SIP_API_AUTH_SCHEME");
    if (scheme is not null) o.AuthScheme = scheme;

    var proxyEnabled = Environment.GetEnvironmentVariable("SIP_PROXY_ENABLED");
    if (bool.TryParse(proxyEnabled, out var proxyEnabledValue)) o.ProxyEnabled = proxyEnabledValue;

    var proxyUrl = Environment.GetEnvironmentVariable("SIP_PROXY_URL");
    if (!string.IsNullOrWhiteSpace(proxyUrl)) o.ProxyUrl = proxyUrl;

    var proxyDefaultCredentials = Environment.GetEnvironmentVariable("SIP_PROXY_USE_DEFAULT_CREDENTIALS");
    if (bool.TryParse(proxyDefaultCredentials, out var proxyCredentialValue)) o.ProxyUseDefaultCredentials = proxyCredentialValue;

    var useDefaultCredentials = Environment.GetEnvironmentVariable("SIP_API_USE_DEFAULT_CREDENTIALS");
    if (bool.TryParse(useDefaultCredentials, out var defaultCredentialValue)) o.UseDefaultCredentials = defaultCredentialValue;
});

builder.Services.AddHttpClient<SipApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SipApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 60 : options.TimeoutSeconds);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
})
.AddHttpMessageHandler<DynamicProxyHandler>()
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseProxy = false });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapGet("/api/files/incidents/{id:int}", async (int id, string? name, SipApiClient sip, CancellationToken ct) =>
{
    var file = await sip.DownloadFileAsync($"incidents/file/{id}", ct);
    name = string.IsNullOrWhiteSpace(name) ? file.FileName : name;
    name = string.IsNullOrWhiteSpace(name) ? $"sip-ek-{id}" : Path.GetFileName(name);
    return Results.File(file.Bytes, file.ContentType, fileDownloadName: name, enableRangeProcessing: true);
});

app.MapGet("/api/files/announcements/{id:int}", async (int id, string? name, SipApiClient sip, CancellationToken ct) =>
{
    var file = await sip.DownloadFileAsync($"announcements/file/{id}", ct);
    name = string.IsNullOrWhiteSpace(name) ? file.FileName : name;
    name = string.IsNullOrWhiteSpace(name) ? $"sip-duyuru-eki-{id}" : Path.GetFileName(name);
    return Results.File(file.Bytes, file.ContentType, fileDownloadName: name, enableRangeProcessing: true);
});

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
