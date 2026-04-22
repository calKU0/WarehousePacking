using System.Net;

namespace WarehousePacking.Server.Services;

public sealed class DatabaseClientHeadersHandler : DelegatingHandler
{
    private const string ClientIpHeader = "X-Client-Ip";
    private const string ClientMachineHeader = "X-Client-Machine";

    private static readonly string MachineName = Environment.MachineName;
    private static readonly string MachineIp = ResolveMachineIp();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove(ClientIpHeader);
        request.Headers.Remove(ClientMachineHeader);

        request.Headers.TryAddWithoutValidation(ClientIpHeader, MachineIp);
        request.Headers.TryAddWithoutValidation(ClientMachineHeader, MachineName);

        return base.SendAsync(request, cancellationToken);
    }

    private static string ResolveMachineIp()
    {
        try
        {
            var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName())
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.ToString())
                .ToList();

            var privateIp = hostAddresses.FirstOrDefault(IsPrivateIpAddress);
            if (!string.IsNullOrWhiteSpace(privateIp))
            {
                return privateIp;
            }

            return hostAddresses.FirstOrDefault() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    private static bool IsPrivateIpAddress(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            return false;
        }

        var bytes = ip.GetAddressBytes();

        if (bytes.Length != 4)
        {
            return false;
        }

        return bytes[0] == 10
            || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            || (bytes[0] == 192 && bytes[1] == 168);
    }
}
