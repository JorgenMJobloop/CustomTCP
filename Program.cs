using System.Text;

namespace TCPServ;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 0)
        {
            Console.WriteLine(string.Join(" ", args));

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
            if (args[0] == "http")
            {
                // working specifically with the Http server
                CancellationTokenSource token = new CancellationTokenSource();

                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    token.Cancel();
                };

                string wwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

                Directory.CreateDirectory(wwwroot);

                await File.WriteAllTextAsync(Path.Combine(wwwroot, "index.html"), """
            <!DOCTYPE html>
            <html>
            <head>
                <title>Mini HTTP over TCP</title>
                <link rel="stylesheet" href="/styles.css">
                <script src="/main.js" defer/></script>
            </head>
            <body>
                <h1>Hello</h1>
                <p>This HTML file is being served over raw TCP.</p>
            </body>
            """, Encoding.UTF8);

                await File.WriteAllTextAsync(Path.Combine(wwwroot, "styles.css"), """
            * {
                box-sizing: border-box;
                margin: 0;
            }
            """, Encoding.UTF8);

                await File.WriteAllTextAsync(Path.Combine(wwwroot, "main.js"), """
            console.log("Hello, world");
            """, Encoding.UTF8);

                await File.WriteAllTextAsync(Path.Combine(wwwroot, "data.json"), """
            {
                "message": "hello, world"
            }
            """, Encoding.UTF8);

                var server = new MiniHttpServer(8080, wwwroot);
                await server.StartAsync(token.Token);
            }
            if (args[0] == "help")
            {
                Console.WriteLine("dotnet run 'server' spin up a simple TCP server");
                Console.WriteLine("dotnet run 'client' spin up a simple TCP listener");
                Console.WriteLine("dotnet run 'http' spin up a minimalistic webserver with static file support");
            }
        }
        else
        {
            Console.WriteLine("dotnet run 'help' display helpful commands");
        }
    }
}
