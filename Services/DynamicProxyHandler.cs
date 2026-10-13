using System.Net;

namespace SipPortal.Services;

/// <summary>Proxy ayarlarını her istekte okuyarak uygulama yeniden başlatılmadan değiştirilmesini sağlar.</summary>
public sealed class DynamicProxyHandler : DelegatingHandler
{
    private readonly ProxySettingsStore _settings;
    private readonly SipApiOptions _apiOptions;

    public DynamicProxyHandler(
        ProxySettingsStore settings,
        Microsoft.Extensions.Options.IOptions<SipApiOptions> apiOptions)
    {
        _settings = settings;
        _apiOptions = apiOptions.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri is not null && !request.RequestUri.IsAbsoluteUri)
        {
            request.RequestUri = new Uri(
                _apiOptions.BaseUrl.TrimEnd('/') + "/" + request.RequestUri.ToString().TrimStart('/'));
        }

        try
        {
            var handler = new HttpClientHandler
            {
                UseProxy = _settings.Enabled,
                UseDefaultCredentials = _settings.ApiUseDefaultCredentials
            };

            if (_settings.Enabled)
            {
                if (!Uri.TryCreate(_settings.Url, UriKind.Absolute, out var proxyUri) ||
                    (proxyUri.Scheme != Uri.UriSchemeHttp && proxyUri.Scheme != Uri.UriSchemeHttps))
                {
                    return ProxyUnavailableResponse("Proxy adresi geçersiz. Ayarlar sayfasından adresi ve portu düzeltin.");
                }

                handler.Proxy = new WebProxy(proxyUri)
                {
                    UseDefaultCredentials = _settings.UseDefaultCredentials
                };
            }

            if (!_settings.VerifyTlsCertificate)
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            // İstek kopyalanır; böylece daha önce gönderilmiş HttpRequestMessage yeniden kullanılmaz.
            using var outboundRequest = await CloneRequestAsync(request, cancellationToken);
            using var client = new HttpClient(handler, disposeHandler: true)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));

            using var response = await client.SendAsync(
                outboundRequest,
                HttpCompletionOption.ResponseContentRead,
                timeout.Token);

            var bytes = response.Content is null
                ? Array.Empty<byte>()
                : await response.Content.ReadAsByteArrayAsync(timeout.Token);
            var contentHeaders = response.Content?.Headers.ToArray()
                ?? Array.Empty<KeyValuePair<string, IEnumerable<string>>>();

            var clonedResponse = new HttpResponseMessage(response.StatusCode)
            {
                ReasonPhrase = response.ReasonPhrase,
                Version = response.Version,
                RequestMessage = request,
                Content = new ByteArrayContent(bytes)
            };

            foreach (var header in response.Headers)
                clonedResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);

            foreach (var header in contentHeaders)
                clonedResponse.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clonedResponse;
        }
        catch (HttpRequestException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Erişilemeyen/yanlış proxy uygulamanın Blazor devresini düşürmesin.
            // API çağrısı 502 olarak döner; kullanıcı Ayarlar sayfasına gidip proxy'yi düzeltebilir.
            return ProxyUnavailableResponse(
                $"Proxy veya SİP API bağlantısı kurulamadı. Proxy ayarlarını kontrol edin. Ayrıntı: {ex.Message}");
        }
        catch (UriFormatException ex)
        {
            return ProxyUnavailableResponse(
                $"Proxy adresi biçimi geçersiz. Ayarlar sayfasından adresi ve portu düzeltin. Ayrıntı: {ex.Message}");
        }
    }


    private static HttpResponseMessage ProxyUnavailableResponse(string message)
    {
        return new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway)
        {
            ReasonPhrase = "Proxy bağlantı hatası",
            Content = new StringContent(message, System.Text.Encoding.UTF8, "text/plain")
        };
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage source,
        CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri)
        {
            Version = source.Version,
            VersionPolicy = source.VersionPolicy
        };

        foreach (var header in source.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var option in source.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        if (source.Content is not null)
        {
            var contentBytes = await source.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentClone = new ByteArrayContent(contentBytes);

            foreach (var header in source.Content.Headers)
                contentClone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            clone.Content = contentClone;
        }

        return clone;
    }
}
