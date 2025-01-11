using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using Microsoft.Data.SqlClient;
using MyHTTPServer.models;
using MyHTTPServer.Sessions;
using MyORMLibrary;

namespace MyHTTPServer.EndPoints;

public class MovieEndpoint : BaseEndPoint
{
    private readonly TestORMContext<Movies> _dbContext;

    public MovieEndpoint()
    {
        string connectionString =
            @"Server=localhost; Database=OnlineCinema; User Id=sa; Password=Passw0rd;TrustServerCertificate=true;";
        var connection = new SqlConnection(connectionString);
        _dbContext = new TestORMContext<Movies>(connection);
    }
    
    [Get("catalog/movie")]
    public IHttpResponceResult GetPage(int id)
    {
        if (!SessionStorage.IsAuthorized(Context))
        {
            return Redirect("login");
        }
        
        try
        {
            var film = _dbContext.GetById(id);

            var templatePath = @"Templates\Pages\Movie\index2.html";
            if (!File.Exists(templatePath))
            {
                return Html("<h1>Template was not found</h1>");
            }

            var template = File.ReadAllText(templatePath);
            var templateEngine = new TemplateEngine.TemplateEngine();
            var htmlContent = templateEngine.Render(template, film);
            return Html(htmlContent);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured: {e.Message}");
            return Html($"<h1>Error occured: {e.Message}</h1>");
        }
        
    }
}