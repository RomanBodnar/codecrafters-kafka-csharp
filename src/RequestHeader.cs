using System;
using System.Buffers.Binary;

namespace codecrafters;

public struct RequestHeader {
    public short RequestApiKey;
    public short RequestApiVersion;
    public int CorrelationId;
    // public NullableString ClientId;
    // public CompactArray TagBuffer;

    public int WriteToSpan(Span<byte> bytes) {
        var bytesWritten = 0;
        BinaryPrimitives.WriteInt32BigEndian(bytes[0..4], CorrelationId);
        bytesWritten += sizeof(int);
        return bytesWritten;
    }

}
