namespace SipPortal.Services;

public sealed class SipApiOptions
{
    public string BaseUrl { get; set; } = "https://sip-api.usom.gov.tr/v1";
    public string AuthHeaderName { get; set; } = "Authorization";
    public string AuthScheme { get; set; } = "Bearer";
    public string Token { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 60;
    public bool VerifyTlsCertificate { get; set; } = true;

    // SGK kurum ağı proxy ayarları
    public bool ProxyEnabled { get; set; } = true;
    public string ProxyUrl { get; set; } = "http://proxy.sgk.intra:9090/";
    public bool ProxyUseDefaultCredentials { get; set; } = true;
    public bool UseDefaultCredentials { get; set; } = true;
}
