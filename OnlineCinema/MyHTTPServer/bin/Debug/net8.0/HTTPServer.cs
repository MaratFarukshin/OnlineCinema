using System.Net;
using HttpServerLibrary.Handlers;


namespace HttpServerLibrary
{
    public sealed class HttpServer
    {
        private readonly HttpListener _listener;

        private readonly Handler _staticFilesHandler;
        private readonly Handler _endPointsHandler;

        public HttpServer(string[] prefixes)
        {
            _staticFilesHandler = new StaticFilesHandler();
            _endPointsHandler = new EndPointsHandler();

            _listener = new HttpListener();
            foreach (var prefix in prefixes)
            {
                Console.WriteLine($"Server started on {prefix}");
                _listener.Prefixes.Add(prefix);
            }
        }
        public async Task StartAsync()
        {
            _listener.Start(); // _listener начинает слушать запросы
            while (_listener.IsListening) // пока сервер активен запросы принимаются
            {
                var context = await _listener.GetContextAsync(); // асинхронное ожидание запроса
                var httpRequestContext =
                    new HttpRequestContext(
                        context); // создание экземпляра класса HttpRequestContext с контекстом полученного запроса
                await ProcessRequestAsync(httpRequestContext); // обработка запроса
            }
        }
        private async Task ProcessRequestAsync(HttpRequestContext context)
        {
            _staticFilesHandler.Successor = _endPointsHandler; // цепочка перехода между handler`ами
            _staticFilesHandler.HandleRequest(context);

            Console.WriteLine("link: " +
                              context.Request.Url);
        }
        
        public void Stop()
        {
            _listener.Stop(); // listener перестает слушать запросы
            Console.WriteLine("Server closed");
        }
    }
}