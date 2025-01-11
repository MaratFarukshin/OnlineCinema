
namespace MyHTTPServer.Temlator;

public interface ICustomTemlator
{
    string GetHtmlByTemplate(string template, string name);
    string GetHtmlByTemplate<T>(string template, T obj);
}