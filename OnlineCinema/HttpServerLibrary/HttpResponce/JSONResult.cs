using System.Net;
using System.Text;
using System.Text.Json;
using HttpServerLibrary.HttpResponce;

namespace HttpServerLibrary.HttpResponce;


internal class JsonResult : IHttpResponceResult
{
    private readonly object _data;

    public JsonResult(object data)
    {
        _data = data;
    }

    public void Execute(HttpListenerResponse response)
    {
        var json = JsonSerializer.Serialize(_data);

        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.Headers.Add("Content-Type", "application/json");
        response.ContentLength64 = buffer.Length;
        using Stream output = response.OutputStream;
        
        output.Write(buffer);
        output.Flush();
    }
}