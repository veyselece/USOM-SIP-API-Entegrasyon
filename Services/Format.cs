namespace SipPortal.Services;

public static class Format
{
    public static string Unix(long value)
    {
        if (value <= 0) return "-";
        try { return DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime().ToString("dd.MM.yyyy HH:mm"); }
        catch { return value.ToString(); }
    }

    public static string StatusIncident(int value) => value switch
    {
        1 => "Bildirildi",
        2 => "İşlem Yapılıyor",
        3 => "Değerlendirildi",
        _ => $"Durum {value}"
    };

    public static string StatusVulnerability(int value) => value switch
    {
        1 => "Bildirildi",
        2 => "Okundu",
        3 => "İşlem Yapılıyor",
        4 => "Çözüldü",
        5 => "Kapatıldı",
        _ => $"Durum {value}"
    };

    public static string Level(int value) => value switch
    {
        1 => "Düşük",
        2 => "Orta",
        3 => "Yüksek",
        4 => "Kritik",
        5 => "Bilgi",
        _ => $"Seviye {value}"
    };

    public static string BadgeClassIncident(int value) => value switch
    {
        1 => "badge-info",
        2 => "badge-warn",
        3 => "badge-ok",
        _ => "badge"
    };

    public static string BadgeClassVulnerability(int value) => value switch
    {
        1 => "badge-danger",
        3 => "badge-warn",
        4 => "badge-ok",
        5 => "badge-info",
        _ => "badge-info"
    };

    public static string BadgeClassLevel(int value) => value switch
    {
        4 => "badge-danger",
        3 => "badge-warn",
        5 => "badge-info",
        _ => "badge"
    };
}
