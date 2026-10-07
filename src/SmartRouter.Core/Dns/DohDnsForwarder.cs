using System.Net.Http.Headers;

namespace SmartRouter.Core.Dns;

public class DohDnsForwarder : IDnsForwarder
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

    private readonly string _dohUrl;

    public string Name { get; }
    public string Endpoint => _dohUrl;

    public DohDnsForwarder(string name, string dohUrl)
    {
        Name = name;
        _dohUrl = dohUrl;
    }

    public async Task<byte[]?> ForwardAsync(byte[] queryBuffer, CancellationToken ct = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, _dohUrl);
            req.Content = new ByteArrayContent(queryBuffer);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/dns-message");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/dns-message"));

            using var resp = await HttpClient.SendAsync(req, ct);
            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadAsByteArrayAsync(ct);
            }
        }
        catch
        {
            // DoH failure fallback
        }
        return null;
    }
}
