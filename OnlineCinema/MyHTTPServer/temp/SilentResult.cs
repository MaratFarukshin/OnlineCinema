using System.Net;
using HttpServerLibrary.HttpResponce;

namespace MyHTTPServer.temp;

public class SilentResult : IHttpResponceResult
{
    private readonly string Body;
    public SilentResult(string body)
    {
        Body = body;
    }
    public void Execute(HttpListenerResponse response) // сюда приходит context.Response
    {
        /// responce.Headers.Add("Content-Type", "text/html");
        // byte[] buffer = Encoding.UTF8.GetBytes(_html);
        // response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        using Stream output = response.OutputStream;
        // output.WriteAsync(buffer);
        output.FlushAsync();
    }
}