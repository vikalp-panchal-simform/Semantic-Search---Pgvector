using BookStore.Api.Exceptions;
using Microsoft.Extensions.AI;
using Pgvector;

namespace BookStore.Api.Services;

public class EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> generator) : IEmbeddingService
{
    public Task<Vector> GenerateBookEmbeddingAsync(string title, string description, string author, CancellationToken cancellationToken = default)
    {
        var text = $"{title}. {description} Author: {author}";
        return GenerateVectorAsync(text, cancellationToken);
    }

    public Task<Vector> GenerateQueryEmbeddingAsync(string query, CancellationToken cancellationToken = default)
        => GenerateVectorAsync(query, cancellationToken);

    private async Task<Vector> GenerateVectorAsync(string text, CancellationToken cancellationToken)
    {
        try
        {
            var embedding = await generator.GenerateVectorAsync(text, cancellationToken: cancellationToken);
            return new Vector(embedding.ToArray());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
        {
            throw new DependencyUnavailableException(
                "Ollama",
                "Could not reach Ollama for embedding generation. Confirm the bookstore-ollama container is running and the all-minilm model is pulled.",
                ex);
        }
    }
}
