namespace WarehousePacking.Server.Services;

/// <summary>
/// Per-circuit identity of the station talking to the API: which workstation and
/// which operator. Attached to every outgoing API call so the server-side logs
/// can be attributed without the API having to guess from network metadata.
///
/// Why not a DelegatingHandler: IHttpClientFactory pools handler chains and
/// resolves them outside the caller's scope, so a handler cannot safely read
/// per-circuit services. Instead each API client is registered here once and its
/// default headers are refreshed whenever the identity changes.
/// </summary>
public sealed class ClientContext
{
    public const string StationHeader = "X-Station-Number";
    public const string UsernameHeader = "X-Username";

    private readonly List<HttpClient> _clients = new();

    public string? StationNumber { get; private set; }
    public string? Username { get; private set; }

    /// <summary>
    /// Registers a client so it carries the identity headers, now and after any
    /// later change (login, workstation reconfiguration).
    /// </summary>
    public void Attach(HttpClient client)
    {
        if (!_clients.Contains(client))
        {
            _clients.Add(client);
        }

        ApplyTo(client);
    }

    public void SetStation(string? stationNumber)
    {
        if (StationNumber == stationNumber)
        {
            return;
        }

        StationNumber = stationNumber;
        ApplyToAll();
    }

    public void SetUsername(string? username)
    {
        if (Username == username)
        {
            return;
        }

        Username = username;
        ApplyToAll();
    }

    private void ApplyToAll()
    {
        foreach (var client in _clients)
        {
            ApplyTo(client);
        }
    }

    /// <summary>
    /// Headers are deliberately omitted when unknown (e.g. before login) rather
    /// than sent as a placeholder — the API treats them as optional.
    /// </summary>
    private void ApplyTo(HttpClient client)
    {
        SetHeader(client, StationHeader, StationNumber);
        SetHeader(client, UsernameHeader, Username);
    }

    private static void SetHeader(HttpClient client, string name, string? value)
    {
        client.DefaultRequestHeaders.Remove(name);

        if (!string.IsNullOrWhiteSpace(value))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
        }
    }
}
