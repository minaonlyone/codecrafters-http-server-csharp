using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

Console.WriteLine("Server started on port 4221");

async Task HandleClient(Socket clientSocket)
{
    var buffer = new byte[1024];
    int receivedData = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None); // read request
    var receivedText = ASCIIEncoding.ASCII.GetString(buffer, 0, receivedData); // read request

    // Split the received text into lines
    var linesSplitted = receivedText.Split(new string[] { "\r\n" }, StringSplitOptions.None);

    // The first line is the request line: method, path, and HTTP version
    var requestLine = linesSplitted[0].Split(' ');
    var method = requestLine[0];
    var path = requestLine[1];
    var httpVer = requestLine[2];

    // Determine the response based on the requested path
    if (path.StartsWith("/files/"))
    {
        var argv = Environment.GetCommandLineArgs();
        var currentDirectory = argv[2];
        var fileName = path.Substring(7);
        var filePath = currentDirectory + fileName;
        if (File.Exists(filePath)) {
            var fileContent = File.ReadAllText(filePath);
            // Construct the response
            var responseBody = fileContent;
            var response = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {responseBody.Length}\r\n\r\n{responseBody}";
            await clientSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)), SocketFlags.None);
        }else{
            var response = "HTTP/1.1 404 Not Found\r\n\r\n";
            await clientSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)), SocketFlags.None);
        }
    }
    else if (path.StartsWith("/echo/"))
    {
        // Extract the string from the path
        var echoStr = path.Substring(6);
        // Construct the response
        var responseBody = echoStr;
        var response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {responseBody.Length}\r\n\r\n{responseBody}";
        await clientSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)), SocketFlags.None);
    }
    else if (path == "/")
    {
        // Send a 200 OK response
        var response = "HTTP/1.1 200 OK\r\n\r\n";
        await clientSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)), SocketFlags.None);
    }
    else if (path == "/user-agent")
    {
        // Extract the User-Agent from the request headers
        string userAgent = "";
        foreach (var line in linesSplitted)
        {
            if (line.StartsWith("User-Agent:"))
            {
                userAgent = line.Substring(12).Trim();
                break;
            }
        }
        // Construct the response
        var responseBody = userAgent;
        var response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {responseBody.Length}\r\n\r\n{responseBody}";
        await clientSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)), SocketFlags.None);
    }
    else
    {
        // Send a 404 Not Found response
        var response = "HTTP/1.1 404 Not Found\r\n\r\n";
        await clientSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response)), SocketFlags.None);
    }

    clientSocket.Close();
}

async Task AcceptClientsAsync()
{
    while (true)
    {
        var clientSocket = await server.AcceptSocketAsync();
        _ = HandleClient(clientSocket); // Start handling the client without waiting for it to complete
    }
}

await AcceptClientsAsync();