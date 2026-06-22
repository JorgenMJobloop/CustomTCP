namespace TCPServ;

class Program
{
    static async Task Main(string[] args)
    {
        if (args[0] == "server")
        {
            var server = new CustomTcpServer(5555);
            await server.StartAsync(default);
        }
        if (args[0] == "client")
        {
            var client = new CustomTcpClient();
            await client.SendAsync("localhost", 5555, "Hello!", default);
        }
        if (args.Length != 0)
        {
            Console.WriteLine(string.Join(" ", args));
        }
        else
        {
            // this is our program's entrypoint if no arguments are given to the commandline
        }
    }
}
