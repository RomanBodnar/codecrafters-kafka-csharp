using System;
using System.Buffers.Binary;

namespace codecrafters;

public record struct RequestHeader {
    public short RequestApiKey;
    public short RequestApiVersion;
    public int CorrelationId;

    public int WriteToSpan(Span<byte> bytes) {
        var bytesWritten = 0;
        BinaryPrimitives.WriteInt32BigEndian(bytes[0..4], CorrelationId);
        bytesWritten += sizeof(int);
        return bytesWritten;
    }

}
