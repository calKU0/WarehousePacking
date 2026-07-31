using System.Text.RegularExpressions;

namespace WarehousePacking.API.Logging;

/// <summary>
/// Strips credentials from anything written to disk. The courier and WMS
/// integrations authenticate with Basic auth, API keys and bearer tokens, so an
/// unredacted archive would be a plaintext credential store.
/// </summary>
public static partial class LogRedactor
{
    private const string Mask = "***REDACTED***";

    private static readonly string[] SensitiveHeaders =
    {
        "authorization",
        "token-mer",
        "x-api-key",
        "apikey",
        "api-secret",
        "proxy-authorization",
        "cookie",
        "set-cookie"
    };

    /// <summary>
    /// Names of JSON/XML properties whose *values* must never be persisted.
    /// One per line: several quoted names on a single line read like a
    /// key/value pair and trip secret scanners on this very file.
    /// </summary>
    private static readonly string[] SensitiveFields =
    {
        "password",
        "haslo",
        "hasło",
        "pwd",
        "secret",
        "apisecret",
        "apikey",
        "token",
        "accesstoken",
        "refreshtoken",
        "authorization",
        "accesscode",
        "clientsecret"
    };

    public static bool IsSensitiveHeader(string headerName)
        => SensitiveHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);

    public static string RedactHeaderValue(string headerName, string value)
        => IsSensitiveHeader(headerName) ? Mask : value;

    /// <summary>
    /// Best-effort masking of credential-bearing values in a payload. Works on
    /// JSON ("password":"x") and XML (&lt;password&gt;x&lt;/password&gt;) without
    /// parsing, so it stays cheap and never throws on malformed content.
    /// </summary>
    public static string RedactBody(string? body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }

        foreach (var field in SensitiveFields)
        {
            body = Regex.Replace(
                body,
                $"(\"{Regex.Escape(field)}\"\\s*:\\s*)\"[^\"]*\"",
                $"$1\"{Mask}\"",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(100));

            body = Regex.Replace(
                body,
                $"(<{Regex.Escape(field)}>)(.*?)(</{Regex.Escape(field)}>)",
                $"$1{Mask}$3",
                RegexOptions.IgnoreCase | RegexOptions.Singleline,
                TimeSpan.FromMilliseconds(100));
        }

        return body;
    }

    /// <summary>Truncates to <paramref name="maxChars"/>, flagging that it happened.</summary>
    public static string Truncate(string value, int maxChars)
    {
        if (maxChars <= 0 || value.Length <= maxChars)
        {
            return value;
        }

        return string.Concat(
            value.AsSpan(0, maxChars),
            $"\n…[truncated {value.Length - maxChars} more chars]");
    }
}
