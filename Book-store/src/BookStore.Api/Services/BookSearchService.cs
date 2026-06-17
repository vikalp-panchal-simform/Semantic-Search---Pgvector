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
        var queryValues = queryVector.ToArray();

        var books = await db.Books
            .Where(b => b.Embedding != null)
            .OrderBy(b => b.Embedding!.CosineDistance(queryVector))
            .Take(limit)
            .ToListAsync(cancellationToken);

        return books.Select(book =>
        {
            var bookValues = book.Embedding!.ToArray();
            return new BookSearchResult
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Author = book.Author,
                Similarity = CosineSimilarity(queryValues, bookValues)
            };
        }).ToList();
    }

    private static double CosineSimilarity(float[] left, float[] right)
    {
        double dot = 0;
        double normLeft = 0;
        double normRight = 0;

        for (var i = 0; i < left.Length; i++)
        {
            dot += left[i] * right[i];
            normLeft += left[i] * left[i];
            normRight += right[i] * right[i];
        }

        if (normLeft == 0 || normRight == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(normLeft) * Math.Sqrt(normRight));
    }
}
