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
        int written = 0;

        BinaryPrimitives.WriteInt32BigEndian(span[..4], this.MessageSize);
        written += sizeof(int);

        var headerSize = HeaderV0.WriteToSpan(span[4..]);
        written += headerSize;
        var offset = 4 + headerSize;
        
        BinaryPrimitives.WriteInt16BigEndian(span[offset..(offset + sizeof(short))], ErrorCode);
        written += sizeof(short);

        // overwrite first 4 bytes with an actual size of a message
        MessageSize = written;
        BinaryPrimitives.WriteInt32BigEndian(span[..4], this.MessageSize);

        return bytes;
    }
     
}
