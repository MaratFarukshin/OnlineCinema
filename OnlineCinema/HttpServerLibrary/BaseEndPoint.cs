using System.Net;
using System.Text;
using HttpServerLibrary.HttpResponce;


namespace HttpServerLibrary;

public class BaseEndPoint
{
    public HttpRequestContext Context { get; private set; }

    internal void SetContext(HttpRequestContext context)
    {
        Context = context;
    }

    public IHttpResponceResult Html(string responceText) => new HTMLResult(responceText);
    public IHttpResponceResult Json(object data) => new JsonResult(data);
    
    public IHttpResponceResult Redirect(string location) => new RedirectResult(location);
    
    // TODO redirect method
}