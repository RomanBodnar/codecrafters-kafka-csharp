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
var receivedMessage = Message.FromSpan(read_buffer);

Message message = new();
message.MessageSize = sizeof(int) * 2;
message.HeaderV0.CorrelationId = receivedMessage.HeaderV0.CorrelationId;

byte[] buffer = message.ToArray();

socket.Send(buffer, 0);

public struct Message {
    public int MessageSize;
    public RequestHeader HeaderV0;

    public byte[] ToArray()
    {
        var bytes = new byte[Marshal.SizeOf<Message>()];
        var span = new Span<byte>(bytes);
        BinaryPrimitives.WriteInt32BigEndian(span[..4], this.MessageSize);
        BinaryPrimitives.WriteInt32BigEndian(span[4..8], HeaderV0.CorrelationId);
        return bytes;
    }

    public static Message FromSpan(Span<byte> bytes)
    {
        var s = default(Message);
        s.MessageSize = BinaryPrimitives.ReadInt32BigEndian(bytes[..4]);
        s.HeaderV0 = default;
        s.HeaderV0.RequestApiKey = BinaryPrimitives.ReadInt16BigEndian(bytes[4..6]);
        s.HeaderV0.RequestApiVersion = BinaryPrimitives.ReadInt16BigEndian(bytes[6..8]);
        s.HeaderV0.CorrelationId = BinaryPrimitives.ReadInt32BigEndian(bytes[8..12]);
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