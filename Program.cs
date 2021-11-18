var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", MyHandler.Hello);
app.Run();




class MyHandler
{
public static string Hello()
    { return "Hello World!"; }
}
