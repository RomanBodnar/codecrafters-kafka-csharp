using System.Buffers.Binary;

public record struct ResponseHeader
{
    public int CorrelationId;
    public int WriteToSpan(Span<byte> bytes)
    {
        var bytesWritten = 0;
        BinaryPrimitives.WriteInt32BigEndian(bytes[0..4], CorrelationId);
        bytesWritten += sizeof(int);
        return bytesWritten;
    }
}
