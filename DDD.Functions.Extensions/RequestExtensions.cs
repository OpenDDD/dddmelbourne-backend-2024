using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class RequestExtensions
{
    public static string GetIpAddress(this HttpRequest req, IReadOnlyDictionary<string, object> bindingData)
    {
        // The Functions host proxies requests to the isolated worker and replaces X-Forwarded-For with its own caller
        // (127.0.0.1 on Flex Consumption), so read the header as the host received it from the trigger binding data.
        var forwardedFor = GetOriginalHeader(bindingData, ForwardedForHeader) ?? req.Headers[ForwardedForHeader].ToString();

        // The Azure front end appends the real client IP; earlier entries are client-controlled.
        // Walk from the right and skip any loopback or private hops added inside the platform.
        var clientIp = forwardedFor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Reverse()
            .Select(entry => IPEndPoint.TryParse(entry, out var endPoint) ? endPoint.Address : null)
            .FirstOrDefault(ip => ip != null && !IsInternal(ip));

        return clientIp?.ToString() ?? req.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    private const string ForwardedForHeader = "X-Forwarded-For";

    private static readonly IPNetwork[] InternalNetworks =
    {
        IPNetwork.Parse("10.0.0.0/8"),
        IPNetwork.Parse("172.16.0.0/12"),
        IPNetwork.Parse("192.168.0.0/16"),
        IPNetwork.Parse("169.254.0.0/16"),
        IPNetwork.Parse("fc00::/7"),
        IPNetwork.Parse("fe80::/10"),
    };

    private static bool IsInternal(IPAddress ip)
    {
        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();
        return IPAddress.IsLoopback(ip) || InternalNetworks.Any(network => network.Contains(ip));
    }

    private static string GetOriginalHeader(IReadOnlyDictionary<string, object> bindingData, string headerName)
    {
        if (bindingData == null || !bindingData.TryGetValue("Headers", out var headersJson) || headersJson == null)
            return null;

        var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson.ToString());
        return headers?.FirstOrDefault(h => string.Equals(h.Key, headerName, StringComparison.OrdinalIgnoreCase)).Value;
    }

    public static async Task<T> ReadAsAsync<T>(this HttpRequest req)
    {
        using var reader = new StreamReader(req.Body, req.GetTypedHeaders().ContentType?.Encoding ?? Encoding.UTF8);
        return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
    }
}
