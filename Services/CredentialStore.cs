using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace SipPortal.Services;

/// <summary>
/// Development-only shared API token store backed by the standard .NET User Secrets file.
/// User Secrets are stored outside the project folder and are not encrypted.
/// </summary>
public sealed class CredentialStore
{
    private readonly object _gate = new();
    private readonly string _secretsPath;
    private readonly string _contentRootPath;
    private readonly bool _userSecretsEnabled;
    private string? _token;

    private const string UserSecretsId = "sipportal-v1-veyselece-local-dev";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string? Token { get { lock (_gate) return _token; } }
    public bool IsPersisted => File.Exists(_secretsPath) && ReadStoredToken() is not null;

    public CredentialStore(IHostEnvironment environment, IOptions<SipApiOptions> options)
    {
        _contentRootPath = environment.ContentRootPath;
        _userSecretsEnabled = environment.IsDevelopment();

        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft", "UserSecrets", UserSecretsId);
        Directory.CreateDirectory(directory);
        _secretsPath = Path.Combine(directory, "secrets.json");

        _token = _userSecretsEnabled ? ReadStoredToken() : null;
        if (_userSecretsEnabled && string.IsNullOrWhiteSpace(_token) && !string.IsNullOrWhiteSpace(options.Value.Token))
        {
            // Migrate a legacy appsettings token to User Secrets and remove it from appsettings.json.
            Save(options.Value.Token, _contentRootPath);
        }
    }

    public void Save(string token, string contentRootPath)
    {
        if (!_userSecretsEnabled)
            throw new InvalidOperationException(".NET User Secrets yalnızca Development ortamında kullanılabilir. Üretim için güvenli bir sır yönetimi kaynağı yapılandırın.");
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token boş olamaz.", nameof(token));

        lock (_gate)
        {
            var root = ReadRoot();
            var sipApi = root["SipApi"] as JsonObject ?? new JsonObject();
            root["SipApi"] = sipApi;
            sipApi["Token"] = token.Trim();
            WriteRoot(root);
            _token = token.Trim();
            RemoveLegacyTokenFromAppSettings(contentRootPath);
        }
    }

    /// <summary>Sets a temporary in-memory token for an API test; does not persist it.</summary>
    public void SetTemporary(string? token)
    {
        lock (_gate) _token = string.IsNullOrWhiteSpace(token) ? null : token.Trim();
    }

    public void ClearMemory()
    {
        lock (_gate) _token = null;
    }

    public void DeletePersisted()
    {
        lock (_gate)
        {
            if (!_userSecretsEnabled)
                throw new InvalidOperationException(".NET User Secrets yalnızca Development ortamında kullanılabilir.");
            var root = ReadRoot();
            if (root["SipApi"] is JsonObject sipApi)
            {
                sipApi.Remove("Token");
                if (sipApi.Count == 0) root.Remove("SipApi");
                WriteRoot(root);
            }
            _token = null;
        }
    }

    private string? ReadStoredToken()
    {
        if (!File.Exists(_secretsPath)) return null;
        var root = JsonNode.Parse(File.ReadAllText(_secretsPath)) as JsonObject;
        return root?["SipApi"]?["Token"]?.GetValue<string>();
    }

    private JsonObject ReadRoot()
    {
        if (!File.Exists(_secretsPath)) return new JsonObject();
        return JsonNode.Parse(File.ReadAllText(_secretsPath)) as JsonObject ?? new JsonObject();
    }

    private void WriteRoot(JsonObject root)
    {
        var tempPath = _secretsPath + ".tmp";
        File.WriteAllText(tempPath, root.ToJsonString(JsonOptions));
        File.Move(tempPath, _secretsPath, overwrite: true);
    }

    private static void RemoveLegacyTokenFromAppSettings(string contentRootPath)
    {
        var path = Path.Combine(contentRootPath, "appsettings.json");
        if (!File.Exists(path)) return;

        var root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
        var sipApi = root?["SipApi"] as JsonObject;
        if (sipApi is null || string.IsNullOrEmpty(sipApi["Token"]?.GetValue<string>())) return;

        sipApi["Token"] = "";
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, root!.ToJsonString(JsonOptions));
        File.Move(tempPath, path, overwrite: true);
    }
}
