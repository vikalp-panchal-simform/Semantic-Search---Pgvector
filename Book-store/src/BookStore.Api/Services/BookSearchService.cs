using BookStore.Api.Data;
using BookStore.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace BookStore.Api.Services;

public class BookSearchService(BookStoreDbContext db, IEmbeddingService embeddingService)
{
    public async Task<IReadOnlyList<BookSearchResult>> SearchAsync(string query, int limit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var queryVector = await embeddingService.GenerateQueryEmbeddingAsync(query, cancellationToken);

        return await db.Books
            .Where(b => b.Embedding != null)
            .OrderBy(b => b.Embedding!.CosineDistance(queryVector))
            .Take(limit)
            .Select(b => new BookSearchResult
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                Author = b.Author,
                Similarity = 1 - b.Embedding!.CosineDistance(queryVector)
            })
            .ToListAsync(cancellationToken);
    }
}
