using System.Diagnostics;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace WarehousePacking.API.Logging;

/// <summary>
/// Times every call to an external system, emits one structured log line, and
/// (optionally) archives the full request/response as JSON.
///
/// External integrations are the usual suspects when something misbehaves in
/// production, and they are exactly what we cannot reproduce locally — so the
/// timing is always recorded, while the expensive payload capture is
/// conditional (failures and slow calls by default).
/// </summary>
public sealed class OutboundCallLoggingHandler : DelegatingHandler
{
    private readonly OutboundCallArchive _archive;
    private readonly OutboundCallOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<OutboundCallLoggingHandler> _logger;

    public OutboundCallLoggingHandler(
        OutboundCallArchive archive,
        IOptions<DiagnosticsOptions> options,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OutboundCallLoggingHandler> logger)
    {
        _archive = archive;
        _options = options.Value.OutboundCalls;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var target = ResolveTarget(request);
        var stopwatch = Stopwatch.StartNew();

        HttpResponseMessage? response = null;
        Exception? failure = null;

        try
        {
            response = await base.SendAsync(request, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            failure = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var statusCode = response is null ? (int?)null : (int)response.StatusCode;

            // Always emit the timing line — cheap, and it is what you need to
            // spot a degrading integration.
            LogSummary(target, request, statusCode, elapsedMs, failure);

            if (ShouldArchive(statusCode, elapsedMs, failure))
            {
                await TryArchiveAsync(target, request, response, statusCode, elapsedMs, failure, cancellationToken);
            }
        }
    }

    private void LogSummary(string target, HttpRequestMessage request, int? statusCode, long elapsedMs, Exception? failure)
    {
        using (LogContext.PushProperty("OutboundTarget", target))
        using (LogContext.PushProperty("ElapsedMs", elapsedMs))
        {
            if (failure is not null)
            {
                _logger.LogError(
                    failure,
                    "Outbound {Target} {Method} {Uri} failed after {ElapsedMs} ms.",
                    target, request.Method.Method, request.RequestUri, elapsedMs);
                return;
            }

            var isSlow = elapsedMs >= _options.SlowCallMs;
            var isFailure = statusCode is >= 400;

            if (isFailure || isSlow)
            {
                _logger.LogWarning(
                    "Outbound {Target} {Method} {Uri} responded {StatusCode} in {ElapsedMs} ms.{SlowHint}",
                    target, request.Method.Method, request.RequestUri, statusCode, elapsedMs,
                    isSlow ? " (slow)" : string.Empty);
            }
            else
            {
                _logger.LogInformation(
                    "Outbound {Target} {Method} {Uri} responded {StatusCode} in {ElapsedMs} ms.",
                    target, request.Method.Method, request.RequestUri, statusCode, elapsedMs);
            }
        }
    }

    private bool ShouldArchive(int? statusCode, long elapsedMs, Exception? failure)
    {
        if (!_archive.IsEnabled)
        {
            return false;
        }

        if (!_options.OnlyFailuresAndSlowCalls)
        {
            return true;
        }

        return failure is not null
            || statusCode is >= 400
            || elapsedMs >= _options.SlowCallMs;
    }

    private async Task TryArchiveAsync(
        string target,
        HttpRequestMessage request,
        HttpResponseMessage? response,
        int? statusCode,
        long elapsedMs,
        Exception? failure,
        CancellationToken cancellationToken)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;

            var record = new OutboundCallRecord
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                Target = target,
                Method = request.Method.Method,
                Uri = request.RequestUri?.ToString() ?? string.Empty,
                StationNumber = ReadItem(context, ClientIdentity.StationNumber),
                Username = ReadItem(context, ClientIdentity.Username),
                StatusCode = statusCode,
                ElapsedMs = elapsedMs,
                Error = failure?.ToString(),
                RequestHeaders = CollectHeaders(request.Headers, request.Content?.Headers),
                ResponseHeaders = response is null
                    ? new Dictionary<string, string>()
                    : CollectHeaders(response.Headers, response.Content.Headers),
                RequestBody = await ReadBodyAsync(request.Content, cancellationToken),
                ResponseBody = await ReadBodyAsync(response?.Content, cancellationToken)
            };

            _archive.TryEnqueue(record);
        }
        catch (Exception ex)
        {
            // Diagnostics must never break the call it is observing.
            _logger.LogDebug(ex, "Could not capture outbound call details for {Target}.", target);
        }
    }

    /// <summary>Identity is optional, so a missing value is simply omitted.</summary>
    private static string? ReadItem(HttpContext? context, string key)
        => context?.Items.TryGetValue(key, out var value) == true ? value as string : null;

    private Dictionary<string, string> CollectHeaders(
        System.Net.Http.Headers.HttpHeaders headers,
        System.Net.Http.Headers.HttpHeaders? contentHeaders)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void Add(System.Net.Http.Headers.HttpHeaders? source)
        {
            if (source is null) return;

            foreach (var header in source)
            {
                result[header.Key] = LogRedactor.RedactHeaderValue(header.Key, string.Join(", ", header.Value));
            }
        }

        Add(headers);
        Add(contentHeaders);
        return result;
    }

    /// <summary>
    /// Buffers the content so reading it here does not consume the stream the
    /// caller still needs. Only text-like payloads are captured.
    /// </summary>
    private async Task<string?> ReadBodyAsync(HttpContent? content, CancellationToken cancellationToken)
    {
        if (content is null)
        {
            return null;
        }

        var mediaType = content.Headers.ContentType?.MediaType ?? string.Empty;
        var isTextLike = mediaType.Contains("json", StringComparison.OrdinalIgnoreCase)
            || mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase)
            || mediaType.Contains("text", StringComparison.OrdinalIgnoreCase)
            || mediaType.Contains("x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
            || mediaType.Length == 0;

        if (!isTextLike)
        {
            return $"[skipped {mediaType} payload]";
        }

        await content.LoadIntoBufferAsync(cancellationToken);
        var raw = await content.ReadAsStringAsync(cancellationToken);

        return LogRedactor.Truncate(LogRedactor.RedactBody(raw), _options.MaxBodyChars);
    }

    /// <summary>Friendly name of the integration, set at registration time.</summary>
    public string? TargetName { get; private set; }

    /// <summary>Fluent helper so each typed client can label its own traffic.</summary>
    public OutboundCallLoggingHandler WithTarget(string targetName)
    {
        TargetName = targetName;
        return this;
    }

    /// <summary>
    /// Prefers the name given at registration, falling back to the host so an
    /// entry is always attributable to something.
    /// </summary>
    private string ResolveTarget(HttpRequestMessage request)
        => !string.IsNullOrWhiteSpace(TargetName)
            ? TargetName!
            : request.RequestUri?.Host ?? "unknown";
}
