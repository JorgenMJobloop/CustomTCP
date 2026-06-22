using System.Net;
using System.Net.Sockets;
using System.Text;

public sealed class MiniHttpServer
{
    private readonly TcpListener _listener;
    private readonly string _wwwroot;

    public MiniHttpServer(int port, string wwwroot)
    {
        _listener = new TcpListener(IPAddress.Loopback, port);
        _wwwroot = wwwroot;
    }

    public async Task StartAsync(CancellationToken token)
    {
        _listener.Start();
        Console.WriteLine($"Server running on: http://localhost:8080");

        while (!token.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(token);
            _ = HandleClientAsync(client, token);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        await using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[4096];
            int numBytesRead = await stream.ReadAsync(buffer, token);

            if (numBytesRead == 0)
            {
                return;
            }

            string request = Encoding.UTF8.GetString(buffer, 0, numBytesRead);
            string path = GetPathFromRequest(request);

            // URL-path. example: localhost/something/something.js
            if (path == "/")
            {
                path = "/index.html";
            }

            string filePath = Path.Combine(_wwwroot, path.TrimStart('/'));

            if (!File.Exists(filePath))
            {
                await WriteResponseAsync(
                    stream,
                    "404 Not Found",
                    "text/plain",
                    "Not found", token
                );
                return;
            }

            string contentType = GetContentType(filePath);
            byte[] body = await File.ReadAllBytesAsync(filePath, token);

            await WriteResponseAsync(
                stream,
                "200 OK",
                contentType,
                body,
                token
            );
        }
    }

    private static string GetPathFromRequest(string request)
    {
        string firstLine = request.Split("\r\n")[0];
        string[] parts = firstLine.Split(' ');
        return parts.Length >= 2 ? parts[1] : "/";
    }

    private static async Task WriteResponseAsync(NetworkStream stream, string status, string contentType, string text, CancellationToken token)
    {
        byte[] body = Encoding.UTF8.GetBytes(text);

        await WriteResponseAsync(
            stream, status, contentType, body, token
        );
    }

    private static async Task WriteResponseAsync(NetworkStream stream, string status, string contentType, byte[] body, CancellationToken token)
    {
        string header =
            $"HTTP/1.1 {status}\r\n" +
            $"Content-Type: {contentType}\r\n" +
            $"Content-Length: {body.Length}\r\n" +
            $"Connection: close\r\n" +
            "\r\n";

        byte[] headerBytes = Encoding.UTF8.GetBytes(header);

        await stream.WriteAsync(headerBytes, token);
        await stream.WriteAsync(body, token);
    }

    private static string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            _ => "application/octet-stream"
        };
    }
}