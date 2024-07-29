using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client
var buffer = new byte[1024];
int receivedData = socket.Receive(buffer); // read request
var receivedText = ASCIIEncoding.ASCII.GetString(buffer, 0, receivedData); // read request

// Split the received text into lines
var linesSplitted = receivedText.Split(new string[] { "\r\n" }, StringSplitOptions.None);

// The first line is the request line: method, path, and HTTP version
var requestLine = linesSplitted[0].Split(' ');
var method = requestLine[0];
var path = requestLine[1];
var httpVer = requestLine[2];

// Determine the response based on the requested path
if (path.StartsWith("/echo/")) {
    // Extract the string from the path
    var echoStr = path.Substring(6);
    // Construct the response
    var responseBody = echoStr;
    var response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {responseBody.Length}\r\n\r\n{responseBody}";
    socket.Send(Encoding.UTF8.GetBytes(response));
}else if (path == "/") {
    // Send a 200 OK response
    var response = "HTTP/1.1 200 OK\r\n\r\n";
    socket.Send(Encoding.UTF8.GetBytes(response));
} else {
    // Send a 404 Not Found response
    var response = "HTTP/1.1 404 Not Found\r\n\r\n";
    socket.Send(Encoding.UTF8.GetBytes(response));
}

socket.Close();
server.Stop();
