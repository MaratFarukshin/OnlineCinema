using System.Net;
using MyHTTPServer.Core;
using MyHTTPServer.Handlers;
using MyHttpServer.Services;

namespace MyHttpServer
{
    /// <summary>
    /// HTTP server that listens for requests and processes them
    /// </summary>
    internal sealed class HttpServer
    {
        private readonly HttpListener _listener; // The HttpListener object used for listening requests

        private readonly Handler _staticFilesHandler; // Handler for static files
        private readonly Handler _endPointsHandler; // Handler for  endpoints

        /// <summary>
        /// HTTPServer constructor
        /// </summary>
        /// <param name="prefixes">An array of URLs for _listener to listen on</param>
        public HttpServer(string[] prefixes)
        {
            _staticFilesHandler = new StaticFilesHandler(); // Creates a new instance of the StaticFilesHandler
            _endPointsHandler = new EndPointsHandler(); // Creates a new instance of the EndPointsHandle

            _listener = new HttpListener(); // Creates a new HttpListener
            foreach (var prefix in prefixes)
            {
                Console.WriteLine($"Server started on {prefix}"); // выводит имя префикста на котором запустился сервер
                _listener.Prefixes.Add(prefix); // добавляет префикс в список
            }
        }

        /// <summary>
        /// Starts the HTTP server asynchronously and starts listening for incoming requests
        /// </summary>
        public async Task StartAsync()
        {
            _listener.Start(); // _listener начинает слушать запросы
            while (_listener.IsListening) // пока сервер активен запросы принимаются
            {
                var context = await _listener.GetContextAsync(); // асинхронное ожидание запроса
                var httpRequestContext =
                    new HttpRequestContext(context); // создание экземпляра класса HttpRequestContext с контекстом полученного запроса
                await ProcessRequestAsync(httpRequestContext); // обработка запроса
            }
        }

        /// <summary>
        /// Asynchronously processes an incoming HTTP request 
        /// </summary>
        /// <param name="context">The context of the HTTP request</param>
        private async Task ProcessRequestAsync(HttpRequestContext context)
        {
            _staticFilesHandler.Successor = _endPointsHandler; // цепочка перехода между handler`ами
            _staticFilesHandler.HandleRequest(context);

            Console.WriteLine("Запрашиваемая ссылка: " +
                              context.Request.Url); 

            if (context.Request.HttpMethod == "POST") // обнаружение POST запроса
            {
                Console.WriteLine($"ЧИПИЧАПА: {context.Request.Url}");
                ProcessPostMethod(context,
                    context.Request.Url.ToString().Split(@"/")[^1]); // начало обработки POST запроса
                return;
            }
        }

        /// <summary>
        /// Processes a POST request, extracts email and message, and sends an email using MailService.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <param name="path">The path part of the requested URL.</param>
        private async void ProcessPostMethod(HttpRequestContext context, string path)
        {
            var request = context.Request;
            var body = new StreamReader(request.InputStream).ReadToEnd(); // Чтение тела запроса
            var content = body.Split("&"); // Разделение запроса на почту и сообщение

            if (content.Length != 2) return; // выполняется если запрос поделен неправильно


            var email = content[0].Replace("login=", "").Replace("%40", "@"); // берем почту
            var message = content[1].Replace("password=", ""); // берем пароль

            //проверка правильности запроса
            if (email.Length == 0 || message.Length == 0 || (email.Contains("@") == false && email.Contains("%40")) ||
                !email.Contains("."))
            {
                context.Response.StatusCode = 400; // статус код клиентской ошибки
                context.Response.Close(); // закрытие ответат
                return;
            }

            Console.WriteLine($"EMAIL: {email}");
            Console.WriteLine("Получено сообщение: ");
            Console.WriteLine(body);

            MailService mailService = new MailService(); // создание экземпляра класса MailService для отправки email`а

            Console.WriteLine(email + " | " + message);

            await mailService.SendAsync(email, message, path); // асинхронная отправка сообщений

            Console.WriteLine("Отправлено сообщение");

            context.Response.StatusCode = 200; // статус код удачного выполнения запроса
            context.Response.Close(); // закрытие ответа
        }

        /// <summary>
        /// Stops the HTTP server.
        /// </summary>
        public void Stop()
        {
            _listener.Stop(); // listener перестает слушать запросы
            Console.WriteLine("Server closed"); 
        }
    }
}