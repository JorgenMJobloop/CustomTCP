using System.Net;
using System.Net.Sockets;
using System.Text;

public sealed class CustomTcpServer
{
    // Main server configuration
    private readonly TcpListener _listener;

    public CustomTcpServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }
    // our server's main loop
    public async Task StartAsync(CancellationToken token)
    {
        _listener.Start();
        Console.WriteLine("Server running..");

        try
        {
            while (!token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(token);

                _ = Task.Run(() => HandleClientAsync(client, token), token);
            }
        }
        finally
        {
            // stop the listener.
            _listener.Stop();
        }
    }
    // how the server handles client connections
    private static async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        await using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];

            while (!token.IsCancellationRequested)
            {
                int numBytesRead = await stream.ReadAsync(buffer, token);

                if (numBytesRead == 0)
                {
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, numBytesRead);
                Console.WriteLine($"Recieved: {message}");

                byte[] response = Encoding.UTF8.GetBytes($"Echo: {message}");
                await stream.WriteAsync(response, token);
            }
        }
    }
}