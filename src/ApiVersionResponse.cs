using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace codecrafters;

public record struct ApiVersionResponse
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



        var apiKeysSize = ApiKeys.WriteToSpan(out var apiKeysSpan);
        var newBytes = span[..offset].ToArray().Concat(apiKeysSpan.ToArray()).ToArray();
        var newSpan = new Span<byte>(newBytes);
        
        offset += apiKeysSize;

        // overwrite first 4 bytes with an actual size of a message
        MessageSize = offset;
        BinaryPrimitives.WriteInt32BigEndian(newSpan[..4], MessageSize);

        Console.WriteLine($"Final offset: {offset}");
        foreach(var b in newBytes){
            Console.WriteLine(b);
        }

        return span.ToArray();
    }
}

public record struct ApiKeys {

    public int Length;
    public ApiVersion[] Versions;
    public int ThrottleTimeMs;
    // TAG_BUFFER

    public ApiKeys()
    {
        Length = 1;
        Versions = new ApiVersion[Length];
        Versions[0] = new ApiVersion(18, 0, 4);
        ThrottleTimeMs = 0;
    }

    public int WriteToSpan(out Span<byte> span) {
        int offset = 0;
        var bytes = new byte[sizeof(int)*2 + sizeof(byte) + Length * Marshal.SizeOf<ApiVersion>()];
        span = new Span<byte>(bytes);

        BinaryPrimitives.WriteInt32BigEndian(span, Length);
        offset += sizeof(int);

        foreach (var version in Versions) {
            int written = version.ToSpan(span[offset..]);
            offset += written;
        }

        BinaryPrimitives.WriteInt32BigEndian(span[offset..], ThrottleTimeMs);
        offset += sizeof(int);
        offset += 1;// tag_buffer must be 0


        return offset;
    }
}

public record struct ApiVersion(short ApiKey, short MinVersion, short MaxVersion) 
{
    public int ToSpan(Span<byte> span) {
        var offset = 0;

        BinaryPrimitives.WriteInt16BigEndian(span[offset..2], ApiKey);
        offset += sizeof(short);
        BinaryPrimitives.WriteInt16BigEndian(span[offset..(offset + 2)], MinVersion);
        offset += sizeof(short);
        BinaryPrimitives.WriteInt16BigEndian(span[offset..(offset + 2)], MaxVersion);
        offset += sizeof(short);

        return offset;
    }
}
