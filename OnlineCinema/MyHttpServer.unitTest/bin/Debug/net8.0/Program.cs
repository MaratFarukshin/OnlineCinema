using System.Net;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using MyHttpServer;
using MyHttpServer.Core.Configuration;


namespace MyHTTPServer;

internal static class Program
{
    // TODO: Необходимо реализовать метод GetHtmlByTemplate в классе CustomTemplator таким образом, чтобы он получал 
    // TODO: тип и замену делал по свойствам данного типа 
    // TODO: проверить вышеописанный метод на тесте который писали на паре
    // [Теория] Razor Шаблонизатор
    // [Теория] MsTest
    // [Теория] Moq
    
    
    static async Task Main(string[] args)
    {
        var prefixes = new[] { $"http://{AppConfig.Domain}:{AppConfig.Port}/" };
        var server = new HttpServer(prefixes);

        await server.StartAsync();
    }
}