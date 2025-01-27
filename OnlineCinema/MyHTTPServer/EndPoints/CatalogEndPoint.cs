using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using Microsoft.Data.SqlClient;
using MyHTTPServer.models;
using MyHTTPServer.Sessions;
using MyORMLibrary;

namespace MyHTTPServer.EndPoints;

public class CatalogEndPoint : BaseEndPoint
{
    private readonly TestORMContext<Movies> _dbContext;

    public CatalogEndPoint()
    {
        string connectionString =
            @"Server=localhost; Database=OnlineCinema; User Id=sa; Password=Passw0rd;TrustServerCertificate=true;";
        var connection = new SqlConnection(connectionString);
        _dbContext = new TestORMContext<Movies>(connection);
    }

    [Get("catalog")]
    public IHttpResponceResult GetMoviesPage()
    {
        // if (!SessionStorage.IsAuthorized(Context))
        // {
        //     return Redirect("login");
        // }
        
        try
        {
            var films = _dbContext.GetByAll();

            var templatePath = @"Templates\Pages\Catalog\index.html";
            if (!File.Exists(templatePath))
            {
                return Html("<h1>Template was not found</h1>");
            }

            var template = File.ReadAllText(templatePath);
            var templateEngine = new TemplateEngine.TemplateEngine();
            var data = new
            {
                Items = films.Select(f => new
                {
                    f.id, 
                    f.title, 
                    f.year, 
                    f.description, 
                    f.rating,
                    f.duration,
                    f.country, 
                    f.director, 
                    f.genre,
                    f.poster_url
                }).ToList()
            };

            var htmlContent = templateEngine.Render(template, data);
            return Html(htmlContent);
        }
        catch (Exception ex)
        {
            return Html($"<h1>Error occured: {ex.Message}</h1>");
        }
    }
    
    [Post("catalog")]
    public IHttpResponceResult PostMoviesPage(string genre)
    {
        if (genre == "all")
            return Redirect("catalog");


        var films = _dbContext.GetByAll().Where(x => x.genre == Translator(genre));

        var templatePath = @"Templates\Pages\Catalog\index.html";
        if (!File.Exists(templatePath))
        {
            return Html("<h1>Template was not found</h1>");
        }

        var template = File.ReadAllText(templatePath);
        var templateEngine = new TemplateEngine.TemplateEngine();
        var data = new
        {
            Items = films.Select(f => new
            {
                f.id,
                f.title,
                f.year,
                f.description,
                f.rating,
                f.duration,
                f.country,
                f.director,
                f.genre,
                f.poster_url
            }).ToList()
        };

        var htmlContent = templateEngine.Render(template, data);
        return Html(htmlContent);
    }


    public static string Translator(string genre)
    {
        switch (genre)
        {
            case "drama":
                return "Драма";
            case "comedy":
                return "Комедия";
            case "triller":
                return "Триллер";
            case "horror":
                return "Хоррор";
            case "criminal":
                return "Криминал";
            default:
                return null;
        }
    }

}

