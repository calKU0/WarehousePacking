using System.Net;

namespace WarehousePacking.API.Logging;

public static class ClientRequestInfoResolver
{
    public static ClientRequestInfo Resolve(HttpContext context)
    {
        var ipAddress = ResolveIpAddress(context);
        var machineName = ResolveMachineName(context, ipAddress);

        return new ClientRequestInfo(ipAddress, machineName);
    }

    private static string ResolveIpAddress(HttpContext context)
    {
        if (TryGetHeaderValue(context, "X-Client-Ip", out var ipFromClient))
        {
            return ipFromClient;
        }

        if (TryGetHeaderValue(context, "X-Forwarded-For", out var forwardedFor))
        {
            var firstIp = forwardedFor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstIp))
            {
                return firstIp;
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string ResolveMachineName(HttpContext context, string ipAddress)
    {
        if (TryGetHeaderValue(context, "X-Client-Machine", out var machineName))
        {
            return machineName;
        }

        if (TryGetHeaderValue(context, "X-Client-Hostname", out var hostName))
        {
            return hostName;
        }

        if (TryGetHeaderValue(context, "X-Machine-Name", out var legacyMachineName))
        {
            return legacyMachineName;
        }

        if (IPAddress.TryParse(ipAddress, out var ip))
        {
            try
            {
                return Dns.GetHostEntry(ip).HostName;
            }
            catch
            {
            }
        }

        return "unknown";
    }

    private static bool TryGetHeaderValue(HttpContext context, string headerName, out string value)
    {
        value = string.Empty;

        if (!context.Request.Headers.TryGetValue(headerName, out var headerValue) || string.IsNullOrWhiteSpace(headerValue))
        {
            return false;
        }

        value = headerValue.ToString().Trim();
        return value.Length > 0;
    }
}
