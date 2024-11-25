using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 9092);
server.Start();
using var socket = server.AcceptSocket(); // wait for client

var read_buffer = new Span<byte>(new byte[1024]);
var bytesRead = socket.Receive(read_buffer);

// first I'll use a list which will be converted to an array
// todo: use a struct

int messageSize = sizeof(int) * 2;
int correlationId = 7;
var message = new List<int> {
    messageSize,
    correlationId
};

var messageArr = message.ToArray();
byte[] buffer = new byte[messageArr.Length * sizeof(int)];
Buffer.BlockCopy(messageArr, 0, buffer, 0, buffer.Length);

socket.Send(buffer, 0);

public struct Message {
    public int MessageSize;
    public int CorrelationId;
}
