using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Linq;
using System.Web;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using HttpServerLibrary.HttpResponce;

namespace HttpServerLibrary.Handlers;

/// <summary>
/// Handler for processing HTTP requests endpoints. Uses reflection to discover and register endpoints.
/// Implements the Chain of Responsibility pattern.
/// </summary>
internal class EndPointsHandler : Handler
{
    /// <summary>
    /// Dictionary mapping URLs to their corresponding handler methods. The key is the URL path, and the value is a list of tuples containing the HTTP method,
    /// the handler method's reflection information, and the endpoint type.
    /// </summary>
    private readonly Dictionary<string, List<(HttpMethod method, MethodInfo handler, Type endpointType)>> _routes =
        new();

    /// <summary>
    /// Constructor for EndPointsHandler. Automatically registers endpoints from the executing assembly.
    /// </summary>
    public EndPointsHandler()
    {
        RegisterEndpointsFromAssemblies(new Assembly?[] { Assembly.GetEntryAssembly() });
    }

    /// <summary>
    /// Handles an incoming HTTP request. 
    /// if found matching endpoint, invokes the handler. Otherwise, sending request to other handler in the chain
    /// </summary>
    /// <param name="context">The HttpRequestContext containing information about request</param>=
    public override void HandleRequest(HttpRequestContext context)
    {
        Console.WriteLine("End point handler");

        // Достаем ссылку и тип метода из контекста запроса
        var url = context.Request.Url?.LocalPath.Trim('/');
        var methodType = context.Request.HttpMethod.ToUpperInvariant();


        if (_routes.ContainsKey(url))
        {
            // Ищем handler который соответствует ссылке и http методы
            var route = _routes[url].FirstOrDefault(r =>
                r.method.ToString().Equals(methodType, StringComparison.InvariantCultureIgnoreCase));

            if (route.handler != null)
            {
                var endpointInstance = Activator.CreateInstance(route.endpointType) as BaseEndPoint;

                if (endpointInstance != null)
                {
                    endpointInstance.SetContext(context);

                    var parameters = GetParams(context, route.handler);
                    // Invoke the handler method. 
                    var result = route.handler.Invoke(endpointInstance, parameters) as IHttpResponceResult;
                    result?.Execute(context.Response); // Execute the result


                    //context.Response.Close();


                    // TODO добавить базовый
                }
            }
        }
        // Если нужный маршрут не найдет, переходим к следующему handler`У
        else if (Successor != null)
        {
            Console.WriteLine("switching to next next handler");
            Successor.HandleRequest(context);
        }
    }

    /// <summary>
    /// Registers all endpoints found in the specified assemblies
    /// </summary>
    /// <param name="assemblies">An array of assemblies to scan for endpoints.</param>
    private void RegisterEndpointsFromAssemblies(Assembly?[] assemblies)
    {
        foreach (Assembly? assembly in assemblies)
        {
            // Find all types that inherit from BaseEndPoint and are not abstract.
            var endpointsTypes = assembly.GetTypes()
                .Where(t => typeof(BaseEndPoint).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var endpointType in endpointsTypes)
            {
                // Get all methods of the endpoint type.
                var methods = endpointType.GetMethods();
                foreach (var method in methods)
                {
                    // TODO refactor 
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

    /// <summary>
    /// Registers a single route with its corresponding handler method.
    /// </summary>
    /// <param name="route">The URL route.</param>
    /// <param name="method">The HTTP method.</param>
    /// <param name="handler">The MethodInfo representing the handler method.</param>
    /// <param name="endpointType">The type of the endpoint class.</param>
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
                    // using var reader = new StreamReader(context.Request.InputStream);
                    // string body = reader.ReadToEnd();
                    // var data = HttpUtility.ParseQueryString(body);
                    result.Add(Convert.ChangeType(data[parameter.Name], parameter.ParameterType));
                }
            }
        }
        else
        {
            // Дополнительная обработка для сегментов URL
            var urlSegments = context.Request.Url?.Segments
                .Skip(2) // Пропуск первых двух сегментов
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