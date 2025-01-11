using System.Net;

namespace HttpServerLibrary.HttpResponce;

public interface IHttpResponceResult
{
    void Execute(HttpListenerResponse response);

}