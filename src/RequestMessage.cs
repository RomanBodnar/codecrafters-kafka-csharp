using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace codecrafters;

public struct RequestMessage {
    public int MessageSize;
    public RequestHeader HeaderV0;


    public static RequestMessage FromSpan(Span<byte> bytes)
    {
        var s = default(RequestMessage);
        s.MessageSize = BinaryPrimitives.ReadInt32BigEndian(bytes[..4]);
        s.HeaderV0 = default;
        s.HeaderV0.RequestApiKey = BinaryPrimitives.ReadInt16BigEndian(bytes[4..6]);
        s.HeaderV0.RequestApiVersion = BinaryPrimitives.ReadInt16BigEndian(bytes[6..8]);
        s.HeaderV0.CorrelationId = BinaryPrimitives.ReadInt32BigEndian(bytes[8..12]);
        return s;
    }
}
