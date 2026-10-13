using System.Net;
using System.Text.RegularExpressions;

namespace SipPortal.Services;

/// <summary>
/// USOM içeriklerinde görülebilen HTML, escaped HTML, özel <str> etiketi,
/// HTML entity'leri ve Markdown bağlantılarını işler.
/// Detay ekranında güvenli bir HTML alt kümesi üretir; liste ekranında düz metin üretir.
/// </summary>
public static class HtmlContent
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "hr", "strong", "b", "em", "i", "u", "s",
        "h1", "h2", "h3", "h4", "h5", "h6",
        "ul", "ol", "li", "blockquote", "pre", "code",
        "table", "thead", "tbody", "tfoot", "tr", "th", "td",
        "div", "span", "small", "sup", "sub", "a"
    };

    private static readonly Regex UnsafeBlocksRegex = new(
        "<\\s*(script|style|iframe|object|embed|applet|form|textarea|select|button|meta|link|base)\\b[^>]*>.*?<\\s*/\\s*\\1\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly Regex TagRegex = new(
        "<\\s*(/?)\\s*([a-zA-Z0-9]+)((?:\\s+[^>]*)?)\\s*/?\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex AnchorHrefRegex = new(
        "<a\\b[^>]*\\bhref\\s*=\\s*[\"'](https?://[^\"']+)[\"'][^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex MarkdownLinkRegex = new(
        "\\[([^\\]]+)\\]\\((https?://[^)]+)\\)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex BreakRegex = new(
        "<br\\s*/?>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex BlockEndRegex = new(
        "</(p|div|li|h[1-6]|tr)>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex WhitespaceRegex = new(
        "\\s+",
        RegexOptions.CultureInvariant);

    public static string ToSafeHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var html = Normalize(value);

        // Script vb. aktif içerik bloklarını ve içeriğini tamamen kaldır.
        html = UnsafeBlocksRegex.Replace(html, string.Empty);

        // USOM çıktılarında görülen özel <str> etiketini strong olarak ele al.
        html = Regex.Replace(
            html,
            "</?\\s*str\\s*>",
            match => match.Value.StartsWith("</", StringComparison.Ordinal)
                ? "</strong>"
                : "<strong>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        // Markdown HTTP/HTTPS bağlantılarını güvenli bağlantıya çevir.
        html = MarkdownLinkRegex.Replace(html, match =>
        {
            var text = WebUtility.HtmlEncode(match.Groups[1].Value);
            var href = WebUtility.HtmlEncode(match.Groups[2].Value);
            return $"<a href=\"{href}\" target=\"_blank\" rel=\"noopener noreferrer\">{text}</a>";
        });

        // Yalnızca izin verilen tag'leri bırak.
        html = TagRegex.Replace(html, match =>
        {
            var isClosing = match.Groups[1].Success;
            var tag = match.Groups[2].Value;
            var attributes = match.Groups[3].Value;

            if (!AllowedTags.Contains(tag))
                return string.Empty;

            if (isClosing)
                return $"</{tag.ToLowerInvariant()}>";

            if (tag.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                var hrefMatch = AnchorHrefRegex.Match(match.Value);
                if (hrefMatch.Success &&
                    Uri.TryCreate(hrefMatch.Groups[1].Value, UriKind.Absolute, out var uri) &&
                    (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                     uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
                {
                    var safeHref = WebUtility.HtmlEncode(uri.AbsoluteUri);
                    return $"<a href=\"{safeHref}\" target=\"_blank\" rel=\"noopener noreferrer\">";
                }

                return "<a>";
            }

            // Güvenlik için diğer izin verilen tag'lerde attribute bırakmıyoruz.
            _ = attributes;
            return $"<{tag.ToLowerInvariant()}>";
        });

        return html;
    }

    public static string ToPlainText(string? value, int maxLength = 220)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "-";

        var normalized = Normalize(value);
        normalized = MarkdownLinkRegex.Replace(normalized, "$1 ($2)");

        // Detay HTML'sini üretmeden önce etiketleri güvenli biçimde metne indir.
        normalized = UnsafeBlocksRegex.Replace(normalized, " ");
        normalized = BreakRegex.Replace(normalized, " ");
        normalized = BlockEndRegex.Replace(normalized, " ");
        normalized = TagRegex.Replace(normalized, string.Empty);
        normalized = WebUtility.HtmlDecode(normalized);
        normalized = WhitespaceRegex.Replace(normalized, " ").Trim();

        if (normalized.Length <= maxLength)
            return normalized;

        return normalized[..Math.Max(0, maxLength - 1)].TrimEnd() + "…";
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var html = value
            .Replace("\\<", "<", StringComparison.Ordinal)
            .Replace("\\>", ">", StringComparison.Ordinal)
            .Replace("\\\"", "\"", StringComparison.Ordinal)
            .Replace("\\'", "'", StringComparison.Ordinal);

        return WebUtility.HtmlDecode(html);
    }
}
