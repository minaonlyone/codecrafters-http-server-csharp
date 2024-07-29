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
var receivedText = ASCIIEncoding.ASCII.GetString(buffer); // read request

var linesSplitted = receivedText.Split("\r\n");
var (method,path,httpVer) = (linesSplitted[0] , linesSplitted[1], linesSplitted[2]);
if(path == "/"){
    socket.Send(Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n"));
}else{
    socket.Send(Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n"));
}
