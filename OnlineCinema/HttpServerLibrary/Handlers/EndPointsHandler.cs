using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Linq;
using System.Web;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using HttpServerLibrary.HttpResponce;

namespace HttpServerLibrary.Handlers;

internal class EndPointsHandler : Handler
{
    private readonly Dictionary<string, List<(HttpMethod method, MethodInfo handler, Type endpointType)>> _routes =
        new();
    public EndPointsHandler()
    {
        RegisterEndpointsFromAssemblies(new Assembly?[] { Assembly.GetEntryAssembly() });
    }
    public override void HandleRequest(HttpRequestContext context)
    {
        Console.WriteLine("End point handler");

        var url = context.Request.Url?.LocalPath.Trim('/');
        var methodType = context.Request.HttpMethod.ToUpperInvariant();


        if (_routes.ContainsKey(url))
        {
            var route = _routes[url].FirstOrDefault(r =>
                r.method.ToString().Equals(methodType, StringComparison.InvariantCultureIgnoreCase));

            if (route.handler != null)
            {
                var endpointInstance = Activator.CreateInstance(route.endpointType) as BaseEndPoint;

                if (endpointInstance != null)
                {
                    endpointInstance.SetContext(context);

                    var parameters = GetParams(context, route.handler);
                    
                    var result = route.handler.Invoke(endpointInstance, parameters) as IHttpResponceResult;
                    result?.Execute(context.Response); 
                }
            }
        }
        else if (Successor != null)
        {
            Console.WriteLine("switching to next next handler");
            Successor.HandleRequest(context);
        }
    }
    private void RegisterEndpointsFromAssemblies(Assembly?[] assemblies)
    {
        foreach (Assembly? assembly in assemblies)
        {
            var endpointsTypes = assembly.GetTypes()
                .Where(t => typeof(BaseEndPoint).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var endpointType in endpointsTypes)
            {
                var methods = endpointType.GetMethods();
                foreach (var method in methods)
                {
                    var getAttribute = method.GetCustomAttribute<GetAttribute>();
                    if (getAttribute != null)
                    {
                        RegisterRoute(getAttribute.Route, HttpMethod.Get, method, endpointType);
                    }

                    var postAttribute = method.GetCustomAttribute<PostAttribute>();
                    if (postAttribute != null)
                    {
                        RegisterRoute(postAttribute.Route, HttpMethod.Post, method, endpointType);
                    }
                }
            }
        }
    }
    private void RegisterRoute(string route, HttpMethod method, MethodInfo handler, Type endpointType)
    {
        if (!_routes.ContainsKey(route))
        {
            _routes[route] = new List<(HttpMethod, MethodInfo, Type)>();
        }

        _routes[route].Add((method, handler, endpointType));
    }

    private object?[] GetParams(HttpRequestContext context, MethodInfo handler)
    {
        var parameters = handler.GetParameters();
        var result = new List<object?>();

        if (context.Request.HttpMethod == "GET" || context.Request.HttpMethod == "POST")
        {
            using var reader = new StreamReader(context.Request.InputStream);
            string body = reader.ReadToEnd();
            var data = HttpUtility.ParseQueryString(body);
            foreach (var parameter in parameters)
            {
                if (context.Request.HttpMethod == "GET")
                {
                    result.Add(Convert.ChangeType(context.Request.QueryString[parameter.Name],
                        parameter.ParameterType));
                }
                else if (context.Request.HttpMethod == "POST")
                {
                    result.Add(Convert.ChangeType(data[parameter.Name], parameter.ParameterType));
                }
            }
        }
        else
        {
            var urlSegments = context.Request.Url?.Segments
                .Skip(2) 
                .Select(s => s.Replace("/", ""))
                .ToArray();

            for (int i = 0; i < parameters.Length; i++)
            {
                result.Add(Convert.ChangeType(urlSegments?[i], parameters[i].ParameterType));
            }
        }

        return result.ToArray();
    }
}