using BookStore.Api.Models.Dtos;
using BookStore.Api.Services;

namespace BookStore.Api.Endpoints;

public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/search", async (
            string q,
            int? limit,
            BookSearchService searchService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Results.BadRequest(new { error = "Query parameter 'q' is required." });
            }

            var take = Math.Clamp(limit ?? 5, 1, 20);
            var results = await searchService.SearchAsync(q, take, cancellationToken);
            return Results.Ok(results);
        })
        .WithName("SearchBooks")
        .WithSummary("Semantic search for books using vector similarity");

        group.MapPost("/", async (
            CreateBookRequest request,
            BookService bookService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title)
                || string.IsNullOrWhiteSpace(request.Description)
                || string.IsNullOrWhiteSpace(request.Author))
            {
                return Results.BadRequest(new { error = "Title, description, and author are required." });
            }

            var book = await bookService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/books/{book.Id}", book);
        })
        .WithName("CreateBook")
        .WithSummary("Create a book and store its embedding");

        return group;
    }
}
