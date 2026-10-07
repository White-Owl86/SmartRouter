using System.Net;

namespace SmartRouter.Core.Dns;

public class DnsQuestion
{
    public string Name { get; set; } = string.Empty;
    public DnsRecordType Type { get; set; } = DnsRecordType.A;
    public ushort Class { get; set; } = 1; // IN
}

public class DnsRecord
{
    public string Name { get; set; } = string.Empty;
    public DnsRecordType Type { get; set; } = DnsRecordType.A;
    public ushort Class { get; set; } = 1;
    public uint Ttl { get; set; } = 300;
    public byte[] Data { get; set; } = [];

    public IPAddress? AsIpAddress()
    {
        if (Type == DnsRecordType.A && Data.Length == 4)
            return new IPAddress(Data);
        if (Type == DnsRecordType.AAAA && Data.Length == 16)
            return new IPAddress(Data);
        return null;
    }
}
