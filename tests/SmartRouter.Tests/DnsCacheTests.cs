using SmartRouter.Core.Dns;
using Xunit;

namespace SmartRouter.Tests;

public class DnsCacheTests
{
    [Fact]
    public void DnsCache_SetAndGet_ReturnsCachedDataWithUpdatedId()
    {
        var cache = new DnsCache();
        byte[] dummyResponse = [0x11, 0x22, 0x81, 0x80, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00];

        cache.Set("example.com", DnsRecordType.A, dummyResponse, 60, "93.184.216.34");

        var hit = cache.TryGet("example.com", DnsRecordType.A, 0xABCD, out var cachedBytes, out var ip);

        Assert.True(hit);
        Assert.NotNull(cachedBytes);
        Assert.Equal("93.184.216.34", ip);
        Assert.Equal(0xAB, cachedBytes[0]);
        Assert.Equal(0xCD, cachedBytes[1]);
    }
}
