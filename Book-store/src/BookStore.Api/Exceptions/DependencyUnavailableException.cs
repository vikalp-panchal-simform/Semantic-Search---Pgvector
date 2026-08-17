namespace BookStore.Api.Exceptions;

/// <summary>
/// Raised when a required external dependency (Ollama, PostgreSQL) cannot be reached.
/// Mapped to HTTP 503 by <see cref="Infrastructure.GlobalExceptionHandler"/>.
/// </summary>
public sealed class DependencyUnavailableException : Exception
{
    public string DependencyName { get; }

    public DependencyUnavailableException(string dependencyName, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        DependencyName = dependencyName;
    }
}
