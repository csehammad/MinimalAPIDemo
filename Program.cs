

//Create WebApplication Builder 
var builder = WebApplication.CreateBuilder(args);


//Inject Connection String and Create EFCore DB Context 
builder.Services.AddDbContext<BooksDB>(options =>
{
    options.UseSqlServer(Environment.GetEnvironmentVariable("AzureConnectionString"));

});


//Inject Swagger Services 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] { }}
    });
});

builder.Services.AddSingleton<TokenService>(new TokenService());
builder.Services.AddSingleton<IUserRepositoryService>(new UserRepositoryService());

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

await using var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();


//Use Swagger in application. 
app.UseSwagger();
app.UseSwaggerUI();



app.MapPost("/login", [AllowAnonymous] async ([FromBodyAttribute] UserModel userModel, TokenService tokenService, IUserRepositoryService userRepositoryService, HttpResponse response) =>
{
    var userDto = userRepositoryService.GetUser(userModel);
    if (userDto == null)
    {
        response.StatusCode = 401;
        return;
    }

    var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"], builder.Configuration["Jwt:Issuer"], builder.Configuration["Jwt:Audience"], userDto);
    await response.WriteAsJsonAsync(new { token = token });
    return;
}).Produces(StatusCodes.Status200OK)
.WithName("Login").WithTags("Accounts");


// Sample Endpoint 
app.MapGet("/", () => "Hello! This is .NET 6 Minimal API Demo on Azure App Service").ExcludeFromDescription();

app.MapGet("/AuthorizedResource", (Func<string>)(

    [Authorize] () => "Action Succeeded")


    ).Produces(StatusCodes.Status200OK)
.WithName("Authorized").WithTags("Accounts").RequireAuthorization();

//Get All Books from the Sql Server DB using Paged Methods
app.MapGet("/books", async (BooksDB db) =>

 await db.Books.ToListAsync()

)
.Produces<List<Book>>(StatusCodes.Status200OK)
.WithName("GetAllBooks").WithTags("Getters");

//Get Books by ID from the Sql Server DB 
app.MapGet("/books/{id}", async (BooksDB db, int id) =>

   await db.Books.SingleOrDefaultAsync(s => s.BookID == id) is Book mybook ? Results.Ok(mybook)
   : Results.NotFound()

 )
.Produces<Book>(StatusCodes.Status200OK)
.WithName("GetBookbyID").WithTags("Getters");



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
    [AllowAnonymous] async ([FromBody] Book addbook,[FromServices] BooksDB db, HttpResponse response) =>
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
     [AllowAnonymous] async (int bookID,string bookTitle, [FromServices] BooksDB db, HttpResponse response) =>
    {
        var mybook = db.Books.SingleOrDefault(s => s.BookID == bookID);

        if (mybook == null) return Results.NotFound();

        mybook.Title = bookTitle;
        
        await db.SaveChangesAsync();
        return Results.Created("/books",mybook);

    })

.Produces<Book>(StatusCodes.Status201Created).Produces(StatusCodes.Status404NotFound)
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

 



