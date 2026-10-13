using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using SipPortal.Models;

namespace SipPortal.Services;

public sealed class SipApiClient
{
    private readonly HttpClient _http;
    private readonly SipApiOptions _options;
    private readonly CredentialStore _credentials;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public SipApiClient(HttpClient http, IOptions<SipApiOptions> options, CredentialStore credentials)
    {
        _http = http;
        _options = options.Value;
        _credentials = credentials;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_credentials.Token ?? _options.Token);
    public string EffectiveBaseUrl => _options.BaseUrl.TrimEnd('/') + "/";

    // Tam yetkili olduğunu, token içinde bunu açıkça belirten bir claim yokken varsaymayız.
    public string AccessModeLabel => DetectTokenAccessMode();

    private string DetectTokenAccessMode()
    {
        var token = _credentials.Token ?? _options.Token;
        if (string.IsNullOrWhiteSpace(token)) return "Yetki Durumu Belirsiz";
        var parts = token.Split('.');
        if (parts.Length < 2) return "Yetki Durumu Belirsiz";
        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = doc.RootElement;
            var claimText = string.Join(" ", root.EnumerateObject()
                .Where(p => p.Name.Contains("role", StringComparison.OrdinalIgnoreCase)
                         || p.Name.Contains("permission", StringComparison.OrdinalIgnoreCase)
                         || p.Name.Contains("scope", StringComparison.OrdinalIgnoreCase)
                         || p.Name.Contains("access", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Value.ToString())).ToLowerInvariant();
            if (claimText.Contains("readonly") || claimText.Contains("read_only") || claimText.Contains("read-only")
                || claimText.Contains("reader") || claimText.Contains("okuyucu"))
                return "Okuyucu Yetkili";
            if (claimText.Contains("admin") || claimText.Contains("write") || claimText.Contains("full")
                || claimText.Contains("tam yetki") || claimText.Contains("editor"))
                return "Tam Yetkili";
            return "Yetki Durumu Belirsiz";
        }
        catch { return "Yetki Durumu Belirsiz"; }
    }

    public async Task<ApiListData<T>> GetListAsync<T>(string path, IDictionary<string, string?>? query = null, CancellationToken ct = default)
    {
        var url = BuildUrl(path, query);
        using var request = CreateRequest(HttpMethod.Get, url);
        using var response = await _http.SendAsync(request, ct);
        return await ReadListAsync<T>(response, ct);
    }

    public async Task<T> GetAsync<T>(string path, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Get, path);
        using var response = await _http.SendAsync(request, ct);
        return await ReadSingleAsync<T>(response, ct);
    }

    public async Task<T> PostAsync<T>(string path, object body, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Post, path);
        request.Content = JsonContent.Create(body, options: JsonOptions);
        using var response = await _http.SendAsync(request, ct);
        return await ReadSingleAsync<T>(response, ct);
    }

    public async Task<T> PutAsync<T>(string path, object body, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Put, path);
        request.Content = JsonContent.Create(body, options: JsonOptions);
        using var response = await _http.SendAsync(request, ct);
        return await ReadSingleAsync<T>(response, ct);
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, path);
        using var response = await _http.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task UploadIncidentFilesAsync(int incidentId, IEnumerable<IBrowserFile> files, CancellationToken ct = default)
        => await UploadFilesAsync($"incidents/file/{incidentId}", files, ct);

    public async Task DeleteIncidentFileAsync(int fileId, CancellationToken ct = default)
        => await DeleteAsync($"incidents/file/{fileId}", ct);

    public async Task UploadVulnerabilityMessageAsync(int id, string message, CancellationToken ct = default)
        => await PostAsync<Vulnerability>($"vulnerabilities/message/{id}", new MessageRequest { Message = message }, ct);

    public async Task<Vulnerability> UpdateVulnerabilityOwnersAsync(int id, IEnumerable<VulnerabilityOwnerUpdate> owners, CancellationToken ct = default)
        => await PutAsync<Vulnerability>($"vulnerabilities/owners/{id}", owners.ToList(), ct);

    public async Task<Incident> AddIncidentMessageAsync(int id, string message, CancellationToken ct = default)
        => await PostAsync<Incident>($"incidents/message/{id}", new MessageRequest { Message = message }, ct);

    public async Task<DownloadedApiFile> DownloadFileAsync(string path, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Get, path);
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccessAsync(response, ct);
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName;
        fileName = fileName?.Trim('"');
        return new DownloadedApiFile(bytes, contentType, fileName);
    }

    public async Task<string> GetFileAsTextAsync(string path, CancellationToken ct = default)
    {
        var file = await DownloadFileAsync(path, ct);
        return System.Text.Encoding.UTF8.GetString(file.Bytes);
    }

    public async Task<HealthResult> TestConnectionAsync(CancellationToken ct = default)
    {
        var started = DateTimeOffset.UtcNow;
        var data = await GetListAsync<SomeInfo>("somes", new Dictionary<string, string?>
        {
            ["page"] = "0",
            ["per-page"] = "1"
        }, ct);
        return new HealthResult(true, DateTimeOffset.UtcNow - started, data.TotalCount, null);
    }

    private async Task UploadFilesAsync(string path, IEnumerable<IBrowserFile> files, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        var openedStreams = new List<Stream>();
        try
        {
            foreach (var file in files)
            {
                var stream = file.OpenReadStream(25 * 1024 * 1024, ct);
                openedStreams.Add(stream);
                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
                form.Add(content, "file", Path.GetFileName(file.Name));
            }

            using var request = CreateRequest(HttpMethod.Post, path);
            request.Content = form;
            using var response = await _http.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);
        }
        finally
        {
            foreach (var stream in openedStreams) stream.Dispose();
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("SİP API token yapılandırılmamış. SIP_API_TOKEN ortam değişkenini veya SipApi:Token ayarını doldurun.");

        var request = new HttpRequestMessage(method, path.TrimStart('/'));
        var token = _credentials.Token ?? _options.Token;
        var value = string.IsNullOrWhiteSpace(_options.AuthScheme) ? token : $"{_options.AuthScheme} {token}";
        request.Headers.TryAddWithoutValidation(_options.AuthHeaderName, value);
        return request;
    }

    private static string BuildUrl(string path, IDictionary<string, string?>? query)
    {
        if (query is null || query.Count == 0) return path;
        var parts = query.Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return $"{path}?{string.Join('&', parts)}";
    }

    private static async Task<ApiListData<T>> ReadListAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        await EnsureSuccessAsync(response, ct);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiListResponse<T>>(JsonOptions, ct)
                      ?? throw new InvalidOperationException("SİP API boş yanıt döndürdü.");
        if (!wrapper.Success || wrapper.Data is null)
            throw new InvalidOperationException(FriendlyApiMessage(wrapper.Exception?.Code ?? 0, wrapper.Exception?.Message));
        return wrapper.Data;
    }

    private static async Task<T> ReadSingleAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        await EnsureSuccessAsync(response, ct);
        var wrapper = await response.Content.ReadFromJsonAsync<ApiSingleResponse<T>>(JsonOptions, ct)
                      ?? throw new InvalidOperationException("SİP API boş yanıt döndürdü.");
        if (!wrapper.Success || wrapper.Data is null)
            throw new InvalidOperationException(FriendlyApiMessage(wrapper.Exception?.Code ?? 0, wrapper.Exception?.Message));
        return wrapper.Data;
    }

    private static string FriendlyApiMessage(int code, string? message)
    {
        if (code is 401 or 403 || string.Equals(message, "Geçersiz Token", StringComparison.OrdinalIgnoreCase))
            return "Bu işlem için yetkiniz bulunmuyor. Hesabınız yalnızca görüntüleme yetkisine sahip olabilir.";
        return string.IsNullOrWhiteSpace(message) ? "SİP API isteği başarısız oldu." : message;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync(ct);
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmuyor. Hesabınız yalnızca görüntüleme yetkisine sahip olabilir.");
        throw new HttpRequestException($"SİP API isteği tamamlanamadı (HTTP {(int)response.StatusCode}). Lütfen daha sonra tekrar deneyin.");
    }
}

public sealed record HealthResult(bool Success, TimeSpan Duration, int SomeCount, string? Error);

public sealed record DownloadedApiFile(byte[] Bytes, string ContentType, string? FileName);
