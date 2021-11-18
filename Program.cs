using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BooksDB>(options =>
{
    options.UseSqlServer(Environment.GetEnvironmentVariable("AzureConnectionString"));

});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();



app.MapGet("/", MyHandler.Hello);

app.MapGet("/books", async (BooksDB db) =>

await db.Books.ToListAsync()

)  
.Produces<List<Book>>(StatusCodes.Status200OK)
.WithName("GetAllBooks").WithTags("Getters");


app.MapPost("/books",
    async ([FromBody] Book addbook,[FromServices] BooksDB db, HttpResponse response) =>
    {
        db.Books.Add(addbook);
        await db.SaveChangesAsync();
        response.StatusCode = 200;
        response.Headers.Location = $"books/{addbook.BookID}";

    })
.Accepts<Book>("application/json")
.Produces<Book>(StatusCodes.Status201Created)
.WithName("AddNewBook").WithTags("Setters");


app.Run();

 
class MyHandler
{
public static string Hello()
    { 
        
        return "Hello! This is .NET 6 Minimal API Demo on Azure App Service";


    }
}
public class Book
{ 
public int BookID { get;  }
public string? Title { get; set; }

    public int Year { get; set; }
    public long ISBN { get; set; }
    public DateTime PublishedDate { get; set; } 
    public short Price { get; set; }

    public int AuthorID { get; set; }
}

class BooksDB : DbContext
{
    public BooksDB(DbContextOptions<BooksDB> options) : base(options) { }
    public DbSet<Book> Books => Set<Book>();
}
