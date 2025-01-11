using System.Net;
using System.Text;
using HttpServerLibrary;
using HttpServerLibrary.HttpResponce;

public class RedirectResult : IHttpResponceResult
{
    private readonly string _location;
    public RedirectResult(string location)
    {
        _location = location;
    }
 
    public void Execute(HttpListenerResponse context)
    {
        context.StatusCode = 302;
        context.Headers.Add("Location", _location);
        //context.Redirect(@"http://localhost:6529/dashboard");
        context.Close();
    }
}