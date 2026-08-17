using System.Net.Sockets;
using BookStore.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BookStore.Api.Infrastructure;

/// <summary>
/// Production-style global handler: maps unhandled exceptions to RFC 7807 ProblemDetails.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Client aborted the connection — nothing useful to return.
        if (httpContext.RequestAborted.IsCancellationRequested
            && exception is OperationCanceledException)
        {
            return true;
        }

        var (statusCode, title, detail) = Map(exception);

        logger.LogError(exception, "Unhandled exception ({StatusCode}): {Title}", statusCode, title);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            }
        });
    }

    private static (int StatusCode, string Title, string Detail) Map(Exception exception)
    {
        if (exception is DependencyUnavailableException dep)
        {
            return (
                StatusCodes.Status503ServiceUnavailable,
                $"{dep.DependencyName} unavailable",
                dep.Message);
        }

        var root = exception.GetBaseException();

        return root switch
        {
            NpgsqlException => (
                StatusCodes.Status503ServiceUnavailable,
                "PostgreSQL unavailable",
                "Could not reach PostgreSQL. Confirm the bookstore-postgres container is running on port 5433."),

            HttpRequestException => (
                StatusCodes.Status503ServiceUnavailable,
                "Ollama unavailable",
                "Could not reach Ollama. Confirm the bookstore-ollama container is running on port 11434."),

            SocketException => (
                StatusCodes.Status503ServiceUnavailable,
                "Dependency unavailable",
                "A network dependency could not be reached. Check PostgreSQL and Ollama containers."),

            TaskCanceledException => (
                StatusCodes.Status503ServiceUnavailable,
                "Upstream timeout",
                "A dependency timed out (often Ollama or PostgreSQL). Retry after confirming containers are healthy."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "Something went wrong while processing the request.")
        };
    }
}
