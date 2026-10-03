using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        var content = new StreamContent(req.Body);
        if (MediaTypeHeaderValue.TryParse(req.ContentType, out var contentType))
            content.Headers.ContentType = contentType;
        // The formatter reads synchronously, and Kestrel rejects synchronous reads of the request body.
        await content.LoadIntoBufferAsync();
        return await content.ReadAsAsync<T>();
    }
}
