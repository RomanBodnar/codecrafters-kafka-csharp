using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using codecrafters;


// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 9092);
server.Start();
using var socket = server.AcceptSocket(); // wait for client

var read_buffer = new Span<byte>(new byte[Marshal.SizeOf<RequestMessage>()]);
var bytesRead = socket.Receive(read_buffer);
var receivedMessage = RequestMessage.FromSpan(read_buffer);

ApiVersionResponse message = new();
message.MessageSize = 0;
message.HeaderV0.CorrelationId = receivedMessage.HeaderV0.CorrelationId;

if (receivedMessage.HeaderV0.RequestApiVersion != 4) {
    message.ErrorCode = ErrorCodes.UNSUPPORTED_VERSION;
} else {
    message.ErrorCode = ErrorCodes.NONE;
}

byte[] buffer = message.ToArray();

socket.Send(buffer, 0);


socket.Close();
