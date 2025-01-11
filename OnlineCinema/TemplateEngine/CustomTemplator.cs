namespace MyHTTPServer.Temlator;

public class CustomTemplator : ICustomTemlator
{
    // template = <h1 > name
    // name = name
    public string GetHtmlByTemplate(string template, string name)
    {
        return template.Replace("{name}", name);
    }

    public string GetHtmlByTemplate<T>(string template, T obj)
    {
        var properties = typeof(T).GetProperties();
        var result = template;
        foreach (var property in properties)
        {
            var propertyName = property.Name;
            var propertyValue = property.GetValue(obj)?.ToString();
            result = result.Replace("{" + propertyName + "}", propertyValue);
        }

        return result;
    }
}