using SmartRouter.Core.Dns;
using Xunit;

namespace SmartRouter.Tests;

public class DnsPacketTests
{
    // A standard DNS query packet for "openai.com" Type A
    private static readonly byte[] OpenAiQueryBytes =
    [
        0xAA, 0xBB, // ID = 0xAABB (43707)
        0x01, 0x00, // Flags: Standard query, RD=1
        0x00, 0x01, // QDCOUNT = 1
        0x00, 0x00, // ANCOUNT = 0
        0x00, 0x00, // NSCOUNT = 0
        0x00, 0x00, // ARCOUNT = 0
        0x06, (byte)'o', (byte)'p', (byte)'e', (byte)'n', (byte)'a', (byte)'i',
        0x03, (byte)'c', (byte)'o', (byte)'m',
        0x00,       // End of name
        0x00, 0x01, // Type = A (1)
        0x00, 0x01  // Class = IN (1)
    ];

    [Fact]
    public void Parse_ValidQueryPacket_ExtractsCorrectDomainAndType()
    {
        var packet = DnsPacket.Parse(OpenAiQueryBytes);

        Assert.NotNull(packet);
        Assert.Equal(0xAABB, packet.Id);
        Assert.True(packet.IsQuery);
        Assert.Single(packet.Questions);

        var q = packet.Questions[0];
        Assert.Equal("openai.com", q.Name);
        Assert.Equal(DnsRecordType.A, q.Type);
    }

    [Fact]
    public void RewriteId_UpdatesTransactionIdCorrectly()
    {
        var buffer = (byte[])OpenAiQueryBytes.Clone();
        DnsPacket.RewriteId(buffer, 0x1234);

        Assert.Equal(0x12, buffer[0]);
        Assert.Equal(0x34, buffer[1]);

        var parsed = DnsPacket.Parse(buffer);
        Assert.NotNull(parsed);
        Assert.Equal(0x1234, parsed.Id);
    }
}
