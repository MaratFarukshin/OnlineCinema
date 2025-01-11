using System.Net;
using HttpServerLibrary.Configuration;


namespace HttpServerLibrary.Handlers;

/// <summary>
/// Handler for serving static files from a directory
/// </summary>
public class StaticFilesHandler : Handler
{
    /// <summary>
    /// The path to the directory containing static files
    /// </summary>
    private readonly string _staticDirectoryPath =
        $"{Directory.GetCurrentDirectory()}\\{AppConfig.StaticDirectoryPath}";

    /// <summary>
    /// Handles an HTTP request
    /// </summary>
    /// <param name="context">The context of the HTTP request.</param>
    public override void HandleRequest(HttpRequestContext context)
    {
        Console.WriteLine("Handling request");


        // Проверка того, что запрос типа GET
        bool isGet = context.Request.HttpMethod.Equals("Get", StringComparison.OrdinalIgnoreCase);
        string[]? arr = context.Request.Url?.AbsolutePath.Split('.'); // Проверка на обращение к файлу

        bool isFile = arr?.Length >= 2; //null check
        if (isGet && isFile)
        {
            try
            {
                Console.WriteLine("IS GET & IS FILE");
                string? relativePath = context.Request.Url?.AbsolutePath.Trim('/');
                string filePath = Path.Combine(_staticDirectoryPath,
                    string.IsNullOrEmpty(relativePath)
                        ? "index.html"
                        : relativePath); // Если нет обращения к конкретному файлу, обращаться к index.html
                // TODO try catch 
                // Если файла не существует вернуть 404
                // if (!File.Exists(filePath))
                // {
                //     // TODO: Implement more robust 404 handling with custom error page and proper status code.
                //     filePath = Path.Combine(_staticDirectoryPath, "err404.html");
                //     context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                // }
                //
                byte[] responseFile = File.ReadAllBytes(filePath); // Побитовое чтение html файла
                // Set Content Type based on file extension
                context.Response.ContentType = GetContentType(Path.GetExtension(filePath));
                context.Response.ContentLength64 = responseFile.Length;
                // Write the file content to the response output stream.
                context.Response.OutputStream.WriteAsync(responseFile, 0, responseFile.Length);
                context.Response.OutputStream.Close(); // closing the stream
            }
            catch
            {
                Console.WriteLine("BAD BAD BAD");
            }
        }
        else if (Successor != null)
        {
            // передача запроса к следующему handler`у в "цепи"
            Console.WriteLine("Switch to next handler");
            Successor.HandleRequest(context);
        }
    }

    /// <summary>
    /// Gets the content type of a file based on its extension.
    /// </summary>
    /// <param name="extension">The file extension</param>
    /// <returns>The content type</returns>
    /// <exception cref="ArgumentNullException">Thrown if the extension is null.</exception>
    private static string GetContentType(string? extension)
    {
        if (extension == null)
        {
            throw new ArgumentNullException(nameof(extension), "Extension cannot be null.");
        }

        return extension.ToLower() switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".svg" => "image/svg",
            ".ico" => "image/x-icon",
            ".gif" => "image/gif",
            _ => "application/octet-stream", // Default content type for undefined extensions
        };
    }
}