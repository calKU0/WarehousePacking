using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace WarehousePacking.API.Logging;

/// <summary>
/// Writes outbound-call records to JSON files off the request path.
///
/// Design constraints:
///   - Producers never block or do I/O: <see cref="TryEnqueue"/> is a non-blocking
///     write to a bounded channel and drops the record if the queue is full.
///     Diagnostics must degrade, never throttle the application.
///   - A single background reader owns all file I/O, so there is no lock
///     contention and no thread-pool churn under load.
///   - Disk is bounded twice over: by age (RetentionDays) and by total size
///     (MaxTotalMegabytes, oldest pruned first), so it cannot grow forever.
/// </summary>
public sealed class OutboundCallArchive : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly Channel<OutboundCallRecord> _queue;
    private readonly OutboundCallOptions _options;
    private readonly ILogger<OutboundCallArchive> _logger;
    private readonly string _rootPath;

    private long _droppedCount;
    private DateTime _lastCleanupUtc = DateTime.MinValue;

    public OutboundCallArchive(IOptions<DiagnosticsOptions> options, ILogger<OutboundCallArchive> logger)
    {
        _options = options.Value.OutboundCalls;
        _logger = logger;

        _rootPath = Path.IsPathRooted(_options.Directory)
            ? _options.Directory
            : Path.Combine(AppContext.BaseDirectory, _options.Directory);

        // DropWrite: under a burst we lose the newest diagnostics rather than
        // slowing down real traffic or growing memory without bound.
        _queue = Channel.CreateBounded<OutboundCallRecord>(
            new BoundedChannelOptions(Math.Max(16, _options.QueueCapacity))
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropWrite
            });
    }

    public bool IsEnabled => _options.Enabled;

    /// <summary>Non-blocking hand-off. Returns false if the record was dropped.</summary>
    public bool TryEnqueue(OutboundCallRecord record)
    {
        if (!_options.Enabled)
        {
            return false;
        }

        if (_queue.Writer.TryWrite(record))
        {
            return true;
        }

        // Log the first drop and then every 100th, so a sustained overload
        // leaves a trace without flooding the log itself.
        var dropped = Interlocked.Increment(ref _droppedCount);
        if (dropped == 1 || dropped % 100 == 0)
        {
            _logger.LogWarning("Outbound call archive queue is full; dropped {DroppedCount} record(s).", dropped);
        }

        return false;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(_rootPath);
            Cleanup();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not initialise the outbound call archive at {Path}.", _rootPath);
        }

        // The cancellation must be caught around the enumerator itself, not just
        // the loop body: ReadAllAsync throws on shutdown, and an exception
        // escaping ExecuteAsync would stop the entire host
        // (BackgroundServiceExceptionBehavior.StopHost is the default).
        try
        {
            await foreach (var record in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await WriteAsync(record, stoppingToken);

                    // Housekeeping runs at most hourly, on this same thread, so it
                    // never competes with request handling.
                    if (DateTime.UtcNow - _lastCleanupUtc > TimeSpan.FromHours(1))
                    {
                        Cleanup();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to archive an outbound call to {Target}.", record.Target);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Outbound call archive writer stopped unexpectedly.");
        }

        // Best-effort drain so diagnostics captured just before shutdown are not
        // silently lost. Bounded by the queue capacity, so it cannot hang.
        while (_queue.Reader.TryRead(out var pending))
        {
            try
            {
                await WriteAsync(pending, CancellationToken.None);
            }
            catch
            {
                break;
            }
        }
    }

    private async Task WriteAsync(OutboundCallRecord record, CancellationToken cancellationToken)
    {
        // One folder per day keeps directories small and makes pruning trivial.
        var dayFolder = Path.Combine(_rootPath, record.TimestampUtc.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(dayFolder);

        var status = record.StatusCode?.ToString() ?? (record.Error is not null ? "ERR" : "na");
        var fileName = $"{record.TimestampUtc:HHmmss.fff}_{Sanitize(record.Target)}_{status}_{record.ElapsedMs}ms_{Guid.NewGuid():N}.json"
            .Replace("__", "_");

        var json = JsonSerializer.Serialize(record, JsonOptions);
        await File.WriteAllTextAsync(Path.Combine(dayFolder, fileName), json, cancellationToken);
    }

    /// <summary>Enforces the age limit first, then the total-size ceiling.</summary>
    private void Cleanup()
    {
        _lastCleanupUtc = DateTime.UtcNow;

        try
        {
            var root = new DirectoryInfo(_rootPath);
            if (!root.Exists)
            {
                return;
            }

            var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, _options.RetentionDays));

            foreach (var dayFolder in root.GetDirectories())
            {
                if (DateTime.TryParse(dayFolder.Name, out var folderDate) && folderDate.ToUniversalTime() < cutoff)
                {
                    dayFolder.Delete(recursive: true);
                }
            }

            var maxBytes = (long)Math.Max(1, _options.MaxTotalMegabytes) * 1024 * 1024;
            var files = root.GetFiles("*.json", SearchOption.AllDirectories)
                .OrderBy(f => f.CreationTimeUtc)
                .ToList();

            var total = files.Sum(f => f.Length);
            foreach (var file in files)
            {
                if (total <= maxBytes)
                {
                    break;
                }

                total -= file.Length;
                file.Delete();
            }

            // Drop day folders left empty by pruning.
            foreach (var dayFolder in root.GetDirectories())
            {
                if (!dayFolder.EnumerateFileSystemInfos().Any())
                {
                    dayFolder.Delete();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Outbound call archive cleanup failed.");
        }
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }
}
