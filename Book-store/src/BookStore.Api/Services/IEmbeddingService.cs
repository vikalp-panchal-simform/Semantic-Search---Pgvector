using Pgvector;

namespace BookStore.Api.Services;

public interface IEmbeddingService
{
    Task<Vector> GenerateBookEmbeddingAsync(string title, string description, string author, CancellationToken cancellationToken = default);
    Task<Vector> GenerateQueryEmbeddingAsync(string query, CancellationToken cancellationToken = default);
}
