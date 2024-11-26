using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 9092);
server.Start();
using var socket = server.AcceptSocket(); // wait for client

var read_buffer = new Span<byte>(new byte[Marshal.SizeOf<Message>()]);
var bytesRead = socket.Receive(read_buffer);

var receivedMessage = Message.FromArray(read_buffer.ToArray());

// first I'll use a list which will be converted to an array
// todo: use a struct
var stream = new MemoryStream();
var writer = new BinaryWriter(stream);

//stream.ToArray();
int messageSize = sizeof(int) * 2;
int correlationId = 7;

byte[] buffer = new byte[Marshal.SizeOf<Message>()];
var span = new Span<byte>(buffer);
BinaryPrimitives.WriteInt32BigEndian(span[..4], messageSize);
BinaryPrimitives.WriteInt32BigEndian(span[4..8], correlationId);


socket.Send(buffer, 0);

public struct Message {
    public int MessageSize;
    public RequestHeader HeaderV0;

    public byte[] ToArray()
    {
        throw new NotImplementedException();
    }

// todo: add FromSpan method
    public static Message FromArray(byte[] bytes)
    {
        var reader = new BinaryReader(new MemoryStream(bytes));
        var s = default(Message);
        s.MessageSize = reader.ReadInt32();
        s.HeaderV0 = default;
        s.HeaderV0.RequestApiKey = reader.ReadInt16();
        s.HeaderV0.RequestApiVersion = reader.ReadInt16();
        s.HeaderV0.CorrelationId = reader.ReadInt32();
        return s;
    }
}

public struct RequestHeader {
    public short RequestApiKey;
    public short RequestApiVersion;
    public int CorrelationId;
    public NullableString ClientId;
    public CompactArray TagBuffer;

}

public struct NullableString {
    public short Length;
    /// <summary>
    /// UTF8
    /// </summary>
    public string? Value;
}

public struct CompactArray {
    public int Length;
}