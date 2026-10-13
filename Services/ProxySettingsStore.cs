using System.Text.Json;
using System.Text.Json.Nodes;

namespace SipPortal.Services;

/// <summary>Proxy ayarlarının tek kalıcı kaynağı appsettings.json dosyasıdır.</summary>
public sealed class ProxySettingsStore
{
    private readonly string _appSettingsPath;
    private readonly object _gate = new();
    private readonly SipApiOptions _startupOptions;

    public bool Enabled { get; private set; }
    public string Url { get; private set; }
    public bool UseDefaultCredentials { get; private set; }
    public bool VerifyTlsCertificate { get; }
    public bool ApiUseDefaultCredentials { get; }

    public ProxySettingsStore(IWebHostEnvironment env, Microsoft.Extensions.Options.IOptions<SipApiOptions> options)
    {
        _appSettingsPath = Path.Combine(env.ContentRootPath, "appsettings.json");
        var o = options.Value;
        _startupOptions = o;
        Enabled = o.ProxyEnabled;
        Url = o.ProxyUrl;
        UseDefaultCredentials = o.ProxyUseDefaultCredentials;
        VerifyTlsCertificate = o.VerifyTlsCertificate;
        ApiUseDefaultCredentials = o.UseDefaultCredentials;
    }

    public void Save(bool enabled, string url, bool useDefaultCredentials)
    {
        url = (url ?? string.Empty).Trim();
        if (enabled && (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
            throw new ArgumentException("Proxy adresi http:// veya https:// ile başlayan geçerli bir adres olmalıdır.");

        lock (_gate)
        {
            if (!File.Exists(_appSettingsPath))
                throw new FileNotFoundException("appsettings.json bulunamadı. Dosya yolu: " + _appSettingsPath);

            var root = JsonNode.Parse(File.ReadAllText(_appSettingsPath)) as JsonObject
                ?? throw new InvalidDataException("appsettings.json geçerli bir JSON nesnesi değil.");
            var sipApi = root["SipApi"] as JsonObject;
            if (sipApi is null)
                throw new InvalidDataException("appsettings.json içinde SipApi bölümü bulunamadı.");

            sipApi["ProxyEnabled"] = enabled;
            sipApi["ProxyUrl"] = url;
            sipApi["ProxyUseDefaultCredentials"] = useDefaultCredentials;

            var json = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            var tempPath = _appSettingsPath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _appSettingsPath, true);

            Enabled = enabled;
            Url = url;
            UseDefaultCredentials = useDefaultCredentials;
        }
    }

    /// <summary>Dosyadaki mevcut appsettings.json proxy değerlerini belleğe yeniden yükler.</summary>
    public void ResetToConfiguration()
    {
        lock (_gate)
        {
            var root = JsonNode.Parse(File.ReadAllText(_appSettingsPath));
            var sipApi = root?["SipApi"];
            Enabled = sipApi?["ProxyEnabled"]?.GetValue<bool>() ?? _startupOptions.ProxyEnabled;
            Url = sipApi?["ProxyUrl"]?.GetValue<string>() ?? _startupOptions.ProxyUrl;
            UseDefaultCredentials = sipApi?["ProxyUseDefaultCredentials"]?.GetValue<bool>() ?? _startupOptions.ProxyUseDefaultCredentials;
        }
    }

    // Backward-compatible property for the settings page; there is no separate override file.
    public bool HasSavedOverride => false;
}
