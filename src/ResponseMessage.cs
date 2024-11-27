using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace codecrafters;

public struct ResponseMessage
{
    public int MessageSize;
    public RequestHeader HeaderV0;

    public short ErrorCode;

    public byte[] ToArray()
    {
        var bytes = new byte[Marshal.SizeOf<RequestMessage>()];
        var span = new Span<byte>(bytes);
        BinaryPrimitives.WriteInt32BigEndian(span[..4], this.MessageSize);
        BinaryPrimitives.WriteInt32BigEndian(span[4..8], HeaderV0.CorrelationId);
        BinaryPrimitives.WriteInt16BigEndian(span[8..10], ErrorCode);
        
        return bytes;
    }
     
}
