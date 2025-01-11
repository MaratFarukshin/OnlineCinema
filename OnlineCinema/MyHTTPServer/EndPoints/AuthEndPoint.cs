using System.Net;
using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using Microsoft.Data.SqlClient;
using MyHTTPServer.models;
using MyORMLibrary;
using MyHTTPServer.Sessions;

namespace MyHTTPServer.EndPoints;

public class AuthEndPoint : BaseEndPoint
{
    [Get("login")]
    public IHttpResponceResult LoginGet()
    {
        if (SessionStorage.IsAuthorized(Context)) 
        {
            return Redirect("catalog");
        }

        var file = File.ReadAllText(
            @"Templates/Pages/Auth/sign_in.html");
        return Html(file);
    }

    [Post("login")]
    public IHttpResponceResult LoginPost(string email, string password)
    {
        try
        {
            string connectionString =
                @"Server=localhost; Database=Users; User Id=sa; Password=Passw0rd;TrustServerCertificate=true;";

            var connection = new SqlConnection(connectionString);
            var context = new TestORMContext<User>(connection);

            var user = context.Where($"email = '{email}' AND password = '{password}'").FirstOrDefault();
            if (user == null)
            {
                return Redirect("login");
            }

            string token = Guid.NewGuid().ToString();
            Cookie cookie = new Cookie("session-token", token);
            Context.Response.SetCookie(cookie);

            SessionStorage.SaveSession(token, user.Id.ToString());

            return Redirect("catalog");
        }
        catch
        {
            return Redirect("login");
        }
    }

    [Get("register")]
    public IHttpResponceResult RegisterGet()
    {
        if (SessionStorage.IsAuthorized(Context))
        {
            return Redirect("catalog");
        }

        var file = File.ReadAllText(
            @"Templates/Pages/Auth/sign_up.html");
        return Html(file);
    }

    [Post("register")]
    public IHttpResponceResult RegisterPost(string user_email, string user_password)
    {
        try
        {
            string connectionString =
                @"Server=localhost; Database=Users; User Id=sa; Password=Passw0rd;TrustServerCertificate=true;";

            var connection = new SqlConnection(connectionString);
            var context = new TestORMContext<User>(connection);

            context.ExecuteQuerySingle(
                $"INSERT INTO Users (email, password) VALUES ('{user_email}', '{user_password}')");

            return Redirect("login");
        }
        catch
        {
            return Redirect("register");
        }
    }
}