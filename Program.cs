var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


static Task<string> getToHtml(string url)
{

    var client = new HttpClient();
    var response = client.GetStringAsync(url);
    return response;
}