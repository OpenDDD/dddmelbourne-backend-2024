using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class RequestExtensions
{
    public static string GetIpAddress(this HttpRequest req)
    {
        // The isolated worker sits behind the Functions host proxy, so RemoteIpAddress is the host, not the client.
        // The Azure front end appends the real client IP last; earlier entries are client-controlled.
        var forwardedFor = req.Headers["X-Forwarded-For"].ToString().Split(',').LastOrDefault()?.Trim();
        if (!string.IsNullOrEmpty(forwardedFor))
            return IPEndPoint.TryParse(forwardedFor, out var endPoint) ? endPoint.Address.ToString() : forwardedFor;
        return req.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    public static async Task<T> ReadAsAsync<T>(this HttpRequest req)
    {
        using var reader = new StreamReader(req.Body, req.GetTypedHeaders().ContentType?.Encoding ?? Encoding.UTF8);
        return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
    }
}
