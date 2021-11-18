using APIDemo.Models;
using Microsoft.AspNetCore.Mvc;

//Create WebApplication Builder 
var builder = WebApplication.CreateBuilder(args);


//Inject Connection String and Create EFCore DB Context 
builder.Services.AddDbContext<BooksDB>(options =>
{
    options.UseSqlServer(Environment.GetEnvironmentVariable("AzureConnectionString"));

});


//Inject Swagger Services 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


//Use Swagger in application. 
app.UseSwagger();
app.UseSwaggerUI();


// Sample Endpoint 
app.MapGet("/", () => "Hello! This is .NET 6 Minimal API Demo on Azure App Service").ExcludeFromDescription();



//Get All Books from the Sql Server DB using Paged Methods
app.MapGet("/books", async ( BooksDB db) =>

await db.Books.ToListAsync()


//await db.Books.ToListAsync()

)
.Produces<List<Book>>(StatusCodes.Status200OK)
.WithName("GetAllBooks").WithTags("Getters");



//Get All Books from the Sql Server DB using Paged Methods
app.MapGet("/books_by_page", async (int pageNumber,int pageSize, BooksDB db) =>

await db.Books
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync()


//await db.Books.ToListAsync()

)
.Produces<List<Book>>(StatusCodes.Status200OK)
.WithName("GetBooksByPage").WithTags("Getters");
 


// Add new book to Sql Server DB 
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


// Update existing book title
app.MapPut("/books",
    async (int bookID,string bookTitle, [FromServices] BooksDB db, HttpResponse response) =>
    {
        var mybook = db.Books.SingleOrDefault(s => s.BookID == bookID);

        if (mybook == null) return Results.NotFound();

        mybook.Title = bookTitle;
        
        await db.SaveChangesAsync();
        return Results.Ok();

    })
.Accepts<Book>("application/json")
.Produces<Book>(StatusCodes.Status201Created)
.WithName("UpdateBook").WithTags("Setters");


app.MapGet("/books/search/{query}",
    (string query, BooksDB db) =>
    {
        var _selectedBooks = db.Books.Where(x => x.Title.ToLower().Contains(query.ToLower())).ToList();

        return _selectedBooks.Count>0? Results.Ok(_selectedBooks): Results.NotFound(Array.Empty<Book>());

    })
    .Produces<List<Book>>(StatusCodes.Status200OK)
.WithName("Search").WithTags("Getters");






//Run the application.
app.Run();

 



