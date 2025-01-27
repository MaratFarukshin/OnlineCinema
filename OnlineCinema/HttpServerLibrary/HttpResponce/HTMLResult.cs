using System.Net;
using System.Text;
using HttpServerLibrary.HttpResponce;

namespace HttpServerLibrary.HttpResponce;

internal class HTMLResult : IHttpResponceResult
{
    private readonly string _html;

    public HTMLResult(string html)
    {
        _html = html;
    }
    public void Execute(HttpListenerResponse response)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(_html);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        using Stream output = response.OutputStream;
        output.WriteAsync(buffer);
        output.FlushAsync();
    }
}