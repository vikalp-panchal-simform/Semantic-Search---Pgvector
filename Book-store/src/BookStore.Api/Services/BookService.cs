using BookStore.Api.Data;
using BookStore.Api.Models;
using BookStore.Api.Models.Dtos;

namespace BookStore.Api.Services;

public class BookService(BookStoreDbContext db, IEmbeddingService embeddingService)
{
    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken = default)
    {
        var title = request.Title.Trim();
        var description = request.Description.Trim();
        var author = request.Author.Trim();

        var embedding = await embeddingService.GenerateBookEmbeddingAsync(
            title,
            description,
            author,
            cancellationToken);

        var book = new Book
        {
            Title = title,
            Description = description,
            Author = author,
            Embedding = embedding,
            // Use DateTimeOffset.UtcNow in production to represent an unambiguous point in time.
            CreatedAt = DateTime.UtcNow
        };

        db.Books.Add(book);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(book);
    }

    private static BookResponse ToResponse(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Description = book.Description,
        Author = book.Author,
        CreatedAt = book.CreatedAt
    };
}
