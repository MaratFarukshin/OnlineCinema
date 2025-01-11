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
    // TODO: Необходимо реализовать метод GetHtmlByTemplate в классе CustomTemplator таким образом, чтобы он получал 
    // TODO: тип и замену делал по свойствам данного типа 
    // TODO: проверить вышеописанный метод на тесте который писали на паре
    // [Теория] Razor Шаблонизатор
    // [Теория] MsTest
    // [Теория] Moq
    
    
    
    // TODO необходимо реализовать метод GetHtmlByTemplate в классе CustomTemplator таким образом, чтобы он обрабатывал 
    // шаблон следующего вида:
    // "if(gender){<h1>Да мой господин, {name}</h1>} else {<h1>Да, моя госпожа {name}</h1>}"
    
    
    static async Task Main(string[] args)
    {
        var prefixes = new[] { $"http://{AppConfig.Domain}:{AppConfig.Port}/" };
        var server = new HttpServer(prefixes);

        await server.StartAsync();
    }
}