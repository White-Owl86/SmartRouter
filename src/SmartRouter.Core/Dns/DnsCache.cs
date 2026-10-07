using System.Collections.Concurrent;

namespace SmartRouter.Core.Dns;

public class CacheEntry
{
    public byte[] ResponseBytes { get; set; } = [];
    public DateTime ExpiresAt { get; set; }
    public string ResolvedIp { get; set; } = string.Empty;
}

public class DnsCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _cache.Count;

    public void Set(string domain, DnsRecordType type, byte[] responseBytes, int ttlSeconds, string resolvedIp)
    {
        var key = $"{domain}:{type}";
        var ttl = Math.Max(10, Math.Min(ttlSeconds, 3600)); // Clamp between 10s and 1hr
        _cache[key] = new CacheEntry
        {
            ResponseBytes = responseBytes,
            ExpiresAt = DateTime.UtcNow.AddSeconds(ttl),
            ResolvedIp = resolvedIp
        };
    }

    public bool TryGet(string domain, DnsRecordType type, ushort queryId, out byte[]? responseBytes, out string resolvedIp)
    {
        var key = $"{domain}:{type}";
        if (_cache.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow < entry.ExpiresAt)
            {
                var clone = (byte[])entry.ResponseBytes.Clone();
                DnsPacket.RewriteId(clone, queryId);
                responseBytes = clone;
                resolvedIp = entry.ResolvedIp;
                return true;
            }
            _cache.TryRemove(key, out _);
        }

        responseBytes = null;
        resolvedIp = string.Empty;
        return false;
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
