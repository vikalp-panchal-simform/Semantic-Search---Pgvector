using BookStore.Api.Data;
using BookStore.Api.Models;
using BookStore.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace BookStore.Api.Services;

public class BookService(BookStoreDbContext db, IEmbeddingService embeddingService)
{
    public async Task<BookResponse> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken = default)
    {
        var embedding = await embeddingService.GenerateBookEmbeddingAsync(
            request.Title,
            request.Description,
            request.Author,
            cancellationToken);

        var book = new Book
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Author = request.Author.Trim(),
            Embedding = embedding,
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
