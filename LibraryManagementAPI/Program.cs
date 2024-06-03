using LibraryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LibraryDb")));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library Management API", Version = "v1" });
});

var app = builder.Build();

// Middleware for exception handling
app.UseExceptionHandler("/error");

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management API v1");
});

app.MapGet("/books", async (LibraryContext db) => await db.Book.Include(b => b.AuthorDetails).ToListAsync());

app.MapGet("/books/{id:int}", async (int id, LibraryContext db) =>
{
    var book = await db.Book.Include(b => b.AuthorDetails).FirstOrDefaultAsync(b => b.Id == id);
    return book is not null ? Results.Ok(book) : Results.NotFound(new { message = "Book not found" });
});

app.MapPost("/books", async (Book newBook, LibraryContext db) =>
{
    if (string.IsNullOrEmpty(newBook.Title) || string.IsNullOrEmpty(newBook.Author) || string.IsNullOrEmpty(newBook.ISBN))
    {
        return Results.BadRequest(new { message = "Title, Author, and ISBN are required." });
    }

    db.Book.Add(newBook);
    await db.SaveChangesAsync();
    return Results.Created($"/books/{newBook.Id}", newBook);
});

app.MapPut("/books/{id:int}", async (int id, Book updatedBook, LibraryContext db) =>
{
    if (string.IsNullOrEmpty(updatedBook.Title) || string.IsNullOrEmpty(updatedBook.Author) || string.IsNullOrEmpty(updatedBook.ISBN))
    {
        return Results.BadRequest(new { message = "Title, Author, and ISBN are required." });
    }

    var book = await db.Book.FindAsync(id);
    if (book is null)
    {
        return Results.NotFound(new { message = "Book not found" });
    }
    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.PublicationDate = updatedBook.PublicationDate;
    book.ISBN = updatedBook.ISBN;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/books/{id:int}", async (int id, LibraryContext db) =>
{
    var book = await db.Book.FindAsync(id);
    if (book is null)
    {
        return Results.NotFound(new { message = "Book not found" });
    }
    db.Book.Remove(book);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/authors", async (LibraryContext db) => await db.Authors.Include(a => a.Books).ToListAsync());

app.MapGet("/authors/{id:int}", async (int id, LibraryContext db) =>
{
    var author = await db.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);
    return author is not null ? Results.Ok(author) : Results.NotFound(new { message = "Author not found" });
});

app.MapPost("/authors", async (Author newAuthor, LibraryContext db) =>
{
    if (string.IsNullOrEmpty(newAuthor.Name))
    {
        return Results.BadRequest(new { message = "Name is required." });
    }

    db.Authors.Add(newAuthor);
    await db.SaveChangesAsync();
    return Results.Created($"/authors/{newAuthor.Id}", newAuthor);
});

app.MapPut("/authors/{id:int}", async (int id, Author updatedAuthor, LibraryContext db) =>
{
    if (string.IsNullOrEmpty(updatedAuthor.Name))
    {
        return Results.BadRequest(new { message = "Name is required." });
    }

    var author = await db.Authors.FindAsync(id);
    if (author is null)
    {
        return Results.NotFound(new { message = "Author not found" });
    }
    author.Name = updatedAuthor.Name;
    author.Books = updatedAuthor.Books;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/authors/{id:int}", async (int id, LibraryContext db) =>
{
    var author = await db.Authors.FindAsync(id);
    if (author is null)
    {
        return Results.NotFound(new { message = "Author not found" });
    }
    db.Authors.Remove(author);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapFallback(async context =>
{
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    await context.Response.WriteAsync("Page not found");
});

app.Run();