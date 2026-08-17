using BookStore.Api.Data;
using BookStore.Api.Endpoints;
using BookStore.Api.Infrastructure;
using BookStore.Api.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Pgvector.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddDbContext<BookStoreDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));

var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"]
    ?? throw new InvalidOperationException("Ollama:Endpoint is not configured.");
var embeddingModel = builder.Configuration["Ollama:EmbeddingModel"]
    ?? throw new InvalidOperationException("Ollama:EmbeddingModel is not configured.");

builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ =>
    (IEmbeddingGenerator<string, Embedding<float>>)new OllamaApiClient(new Uri(ollamaEndpoint), embeddingModel));

builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<BookSearchService>();

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: ["ready", "db"])
    .AddUrlGroup(
        new Uri(new Uri(ollamaEndpoint.TrimEnd('/') + "/"), "api/tags"),
        name: "ollama",
        tags: ["ready", "ai"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<BookStoreDbContext>();
//    await db.Database.MigrateAsync();

//    var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
//    await BookSeeder.SeedAsync(db, embeddingService);
//}

app.MapBookEndpoints();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
});

app.Run();
