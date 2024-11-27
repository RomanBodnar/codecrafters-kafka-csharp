using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace codecrafters;

public struct ApiVersionResponse
{
    public ApiVersionResponse() {
        HeaderV0 = new();
		ApiKeys = new();
	}

    public int MessageSize;
    public RequestHeader HeaderV0;
    public short ErrorCode;

    // todo: must be an array
    public ApiKeys ApiKeys;

    public byte[] ToArray()
    {
        var bytes = new byte[Marshal.SizeOf<ApiVersionResponse>()];
        Console.WriteLine($"Allocated array length: {bytes.Length}");
        var span = new Span<byte>(bytes);
        int offset = 0;

        BinaryPrimitives.WriteInt32BigEndian(span[..4], MessageSize);
        offset += 4;

        var headerSize = HeaderV0.WriteToSpan(span[4..]);
        offset += headerSize;

        Console.WriteLine($"Offset after header: {offset}");

        BinaryPrimitives.WriteInt16BigEndian(span[offset..(offset + 2)], ErrorCode);
        offset += sizeof(short);

        var apiKeysSize = ApiKeys.WriteToSpan(span[offset..(offset + 4)]);
        offset += apiKeysSize;

        // overwrite first 4 bytes with an actual size of a message
        MessageSize = offset;
        BinaryPrimitives.WriteInt32BigEndian(span[..4], MessageSize);

        Console.WriteLine($"Final offset: {offset}");
        foreach(var b in bytes[..offset]){
            Console.WriteLine(b);
        }

        return span[..offset].ToArray();
    }
}

public struct ApiKeys {
    public ApiKeys() {
        ApiKey = 18;
        MinVersion = 0;
        MaxVersion = 4;
    }

    public short ApiKey;
    public short MinVersion;
    public short MaxVersion;

    public int WriteToSpan(Span<byte> span) {
        int written = 0;
        // write lenght of an array
        BinaryPrimitives.WriteInt16BigEndian(span[0..2], 1);
        written += sizeof(short);

        BinaryPrimitives.WriteInt16BigEndian(span[2..4], ApiKey);
        written += sizeof(short);
       
        BinaryPrimitives.WriteInt16BigEndian(span[4..6], MaxVersion);
        written += sizeof(short);
        

        //stream.Put(0).Put((byte)0); // throttle time ms + TAG BUFFER of the api-keys body
        
        BinaryPrimitives.WriteInt32BigEndian(span[6..10], 0);
        
        BinaryPrimitives.WriteInt32BigEndian(span[10..14], 0);
        written += sizeof(int) + sizeof(int);
        
        return written;
    }
}
