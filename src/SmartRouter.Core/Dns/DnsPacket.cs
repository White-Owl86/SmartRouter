using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace SmartRouter.Core.Dns;

public class DnsPacket
{
    public ushort Id { get; set; }
    public ushort Flags { get; set; }
    public bool IsQuery => (Flags & 0x8000) == 0;
    public bool IsResponse => !IsQuery;
    public DnsResponseCode ResponseCode => (DnsResponseCode)(Flags & 0x000F);

    public List<DnsQuestion> Questions { get; } = [];
    public List<DnsRecord> Answers { get; } = [];
    public List<DnsRecord> Authorities { get; } = [];
    public List<DnsRecord> Additionals { get; } = [];

    public byte[] RawBytes { get; set; } = [];

    public static DnsPacket? Parse(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 12) return null;

        var packet = new DnsPacket
        {
            Id = BinaryPrimitives.ReadUInt16BigEndian(buffer[0..2]),
            Flags = BinaryPrimitives.ReadUInt16BigEndian(buffer[2..4]),
            RawBytes = buffer.ToArray()
        };

        var qdCount = BinaryPrimitives.ReadUInt16BigEndian(buffer[4..6]);
        var anCount = BinaryPrimitives.ReadUInt16BigEndian(buffer[6..8]);
        var nsCount = BinaryPrimitives.ReadUInt16BigEndian(buffer[8..10]);
        var arCount = BinaryPrimitives.ReadUInt16BigEndian(buffer[10..12]);

        var offset = 12;

        // Parse Questions
        for (var i = 0; i < qdCount; i++)
        {
            var name = ReadName(buffer, ref offset);
            if (name == null || offset + 4 > buffer.Length) return packet;

            var qtype = (DnsRecordType)BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(offset, 2));
            var qclass = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(offset + 2, 2));
            offset += 4;

            packet.Questions.Add(new DnsQuestion
            {
                Name = name,
                Type = qtype,
                Class = qclass
            });
        }

        // Parse Answers
        for (var i = 0; i < anCount; i++)
        {
            var record = ReadRecord(buffer, ref offset);
            if (record != null) packet.Answers.Add(record);
            else break;
        }

        return packet;
    }

    public static void RewriteId(byte[] rawPacket, ushort newId)
    {
        if (rawPacket.Length >= 2)
        {
            BinaryPrimitives.WriteUInt16BigEndian(rawPacket.AsSpan(0, 2), newId);
        }
    }

    private static DnsRecord? ReadRecord(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var name = ReadName(buffer, ref offset);
        if (name == null || offset + 10 > buffer.Length) return null;

        var type = (DnsRecordType)BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(offset, 2));
        var rClass = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(offset + 2, 2));
        var ttl = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(offset + 4, 4));
        var rdLength = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(offset + 8, 2));
        offset += 10;

        if (offset + rdLength > buffer.Length) return null;

        var data = buffer.Slice(offset, rdLength).ToArray();
        offset += rdLength;

        return new DnsRecord
        {
            Name = name,
            Type = type,
            Class = rClass,
            Ttl = ttl,
            Data = data
        };
    }

    private static string? ReadName(ReadOnlySpan<byte> buffer, ref int offset)
    {
        var sb = new StringBuilder();
        var jumped = false;
        var jumpOffset = -1;
        var maxJumps = 10;
        var currentOffset = offset;

        while (true)
        {
            if (currentOffset >= buffer.Length) return null;
            var len = buffer[currentOffset++];

            if (len == 0)
            {
                break;
            }

            // Pointer compression: top 2 bits are 11 (0xC0)
            if ((len & 0xC0) == 0xC0)
            {
                if (currentOffset >= buffer.Length) return null;
                var b2 = buffer[currentOffset++];
                var pointer = ((len & 0x3F) << 8) | b2;

                if (!jumped)
                {
                    jumpOffset = currentOffset;
                    jumped = true;
                }

                if (--maxJumps <= 0) return null; // Avoid infinite loop
                currentOffset = pointer;
                continue;
            }

            if (currentOffset + len > buffer.Length) return null;

            if (sb.Length > 0) sb.Append('.');
            sb.Append(Encoding.ASCII.GetString(buffer.Slice(currentOffset, len)));
            currentOffset += len;
        }

        offset = jumped ? jumpOffset : currentOffset;
        return sb.ToString();
    }
}
