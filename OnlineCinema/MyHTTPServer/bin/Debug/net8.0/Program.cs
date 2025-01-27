using System.Net;
using System.Text;
using System.IO;
using System.Net.Sockets;
using HttpServerLibrary;
using MyHttpServer;
using HttpServerLibrary.Configuration;


namespace MyHTTPServer;

public static class Program
{
    
    
    static async Task Main(string[] args)
    {
        var prefixes = new[] { $"http://{AppConfig.Domain}:{AppConfig.Port}/" };
        var server = new HttpServer(prefixes);

        await server.StartAsync();
    }
}