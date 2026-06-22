using System.Net.Sockets;
using System.Text;

public sealed class CustomTcpClient
{
    public async Task SendAsync(string host, int port, string message, CancellationToken token)
    {
        using var client = new TcpClient();

        await client.ConnectAsync(host, port, token);

        await using NetworkStream stream = client.GetStream();

        byte[] data = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(data, token);

        byte[] buffer = new byte[1024];
        int numBytesRead = await stream.ReadAsync(buffer, token);

        string response = Encoding.UTF8.GetString(buffer, 0, numBytesRead);
        Console.WriteLine(response);
    }
}