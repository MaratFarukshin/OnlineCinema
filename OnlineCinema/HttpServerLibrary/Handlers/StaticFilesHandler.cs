using System.Net;
using HttpServerLibrary.Configuration;


namespace HttpServerLibrary.Handlers;

/// <summary>
/// Handler for serving static files from a directory
/// </summary>
public class StaticFilesHandler : Handler
{
    private readonly string _staticDirectoryPath =
        $"{Directory.GetCurrentDirectory()}\\{AppConfig.StaticDirectoryPath}";

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
                string? relativePath = context.Request.Url?.AbsolutePath.Trim('/');
                string filePath = Path.Combine(_staticDirectoryPath,
                    string.IsNullOrEmpty(relativePath)
                        ? "index.html"
                        : relativePath);
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
            }
        }
        else if (Successor != null)
        {
            // передача запроса к следующему handler`у в "цепи"
            Successor.HandleRequest(context);
        }
    }
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