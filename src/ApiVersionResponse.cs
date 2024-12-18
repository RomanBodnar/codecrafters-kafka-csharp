using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace codecrafters;

public record struct ApiVersionResponse
{
    public int MessageSize;
    public ResponseHeader HeaderV0;
    public short ErrorCode;
    public ApiKeys ApiKeys;

    public ApiVersionResponse() {
        HeaderV0 = new();
		ApiKeys = new();
	}

    public byte[] ToByteArray()
    {
        using var stream = new MemoryStream();
        MessageSize = 19;
        stream.Put(MessageSize);

        Console.WriteLine($"HEADER: {Marshal.SizeOf<ResponseHeader>()}");
        stream.Put(HeaderV0.CorrelationId); 
        stream.Put(ErrorCode); 
        if (ApiKeys.Length > 0)
        {
            stream.Put((byte)(ApiKeys.Length + 1));
            foreach (var key in ApiKeys.Versions)
            {
                stream.Put(key.ApiKey); 
                stream.Put(key.MinVersion);
                stream.Put(key.MaxVersion); 
                stream.Put((byte)0); 
            }
        }
        else {
            stream.Put((byte)(ApiKeys.Length + 1));
        }

        stream.Put(ApiKeys.ThrottleTimeMs);
        stream.Put((byte)0);
        return stream.ToArray();
    }
    public byte[] ToArray()
    {
        var bytes = new byte[1024];
       
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
        apiKeysSpan.CopyTo(span[offset..]);

        offset += apiKeysSize;

        // overwrite first 4 bytes with an actual size of a message
        MessageSize = offset;
        BinaryPrimitives.WriteInt32BigEndian(span[..4], MessageSize);

        Console.WriteLine($"Final offset: {offset}");
       
        Console.WriteLine();
        var resultSpan = new Span<byte>(new byte[MessageSize]);
        for(int i = 0; i < resultSpan.Length; i++) {
            resultSpan[i] = span[i];

        }
         foreach (var b in resultSpan.ToArray())
        {
            Console.Write(b + " ");
        }
        //span.CopyTo(resultSpan);
        return new ArraySegment<byte>(span.ToArray(), 0, 23).ToArray();
    }
}

public record struct ApiKeys {

    public sbyte Length;
    public ApiVersion[] Versions;
    public int ThrottleTimeMs;
    // TAG_BUFFER

    public ApiKeys()
    {
        Length = 1;
        Versions = new ApiVersion[Length];
        Versions[0] = new ApiVersion(18, 0, 4);
        ThrottleTimeMs = 500;
    }

    public int WriteToSpan(out Span<byte> span) {
        /*
ApiVersions Response (Version: CodeCrafters) 
=> error_code num_of_api_keys [api_keys] throttle_time_ms TAG_BUFFER
    error_code => INT16
    num_of_api_keys => INT8
    api_keys => api_key min_version max_version
        api_key => INT16
        min_version => INT16
        max_version => INT16
        _tagged_fields
    throttle_time_ms => INT32
    _tagged_fields


        Represents a sequence of objects of a given type T. Type T can be either a primitive type (e.g. STRING) or a structure. First, the length N + 1 is given as an UNSIGNED_VARINT. Then N instances of type T follow. A null array is represented with a length of 0. In protocol documentation an array of T instances is referred to as [T].

I don�t understand why they do this instead of just using 0 for 0, 1 for 1, � but adding this extra + 1 for non empty arrays made it work

Regarding the TAG_BUFFER, they said we don�t use tag in the CodeCrafter challenge, so it os just one byte to encode a length of 0 for the tag array. At least, that�s the more or less logical explanation I came up with after being puzzled just like you are and deriving this 0 on one byte empirically TAG_BUFFER explanation � romainfd/codecrafters-kafka-python@8b3ecf9 � GitHub
        https://github.com/romainfd/codecrafters-kafka-python/commit/8b3ecf9aa78df0a5098d0463d46e997f6581aac7
So I agree the instructions are not super clear but I think this is also a problem of the Kafka doc not always being super clear�

I also had a LOT of trouble finding out when to use Response Header V0 vs V1�

        https://forum.codecrafters.io/t/question-about-handle-apiversions-requests-stage/1743/8
         */
        int offset = 0;
        var bytes = new byte
        [
            sizeof(int) + // throttle ms
            sizeof(byte) * 2 + // length + tag
            Length * (Marshal.SizeOf<ApiVersion>() + 1) // api versions + tag buffers 
        ];
        span = new Span<byte>(bytes);

        span[0] = (byte)(Length + 1);
        offset += sizeof(byte);
        if(Length > 0) {
            foreach (var version in Versions) {
                int written = version.ToSpan(span[offset..]);
                offset += written;
            }
        }
        BinaryPrimitives.WriteInt32BigEndian(span[offset..], ThrottleTimeMs);
        offset += sizeof(int);

        span[offset] = (byte)0;
        offset += 1;// tag_buffer must be 0

        Console.WriteLine("api keys");
        foreach (var b in bytes)
        {
            Console.Write(b + " ");
        }
        Console.WriteLine();

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

        span[offset] = 0;
        offset += 1; // tag buffer

        return offset;
    }
}
