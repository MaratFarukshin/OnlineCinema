namespace MyHTTPServer.models;

public class Movies
{
    public int id { get; set; }
    public string title { get; set; }
    public int year { get; set; }
    public string description { get; set; }
    public double rating { get; set; }
    public int duration { get; set; }
    public string country { get; set; }
    
    public string director { get; set; }
    public string genre { get; set; }
    public string poster_url { get; set; }
}