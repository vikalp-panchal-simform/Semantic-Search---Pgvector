using BookStore.Api.Models;
using BookStore.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Data;

public static class BookSeeder
{
    private static readonly (string Title, string Author, string Description)[] SeedBooks =
    [
        // Animal Behavior Books
        ("Dog Training for Beginners", "Sarah Wilson", "Learn positive reinforcement, puppy obedience, leash training, and common dog behavior techniques."),
        ("Understanding Canine Psychology", "Mark Peterson", "Explore dog emotions, pack behavior, communication signals, and effective training methods."),
        ("The Hidden Life of Wolves", "David Mech", "Study wolf packs, hunting strategies, social hierarchy, and survival in the wild."),
        ("Marine Mammals Explained", "Thomas Jefferson", "Discover dolphins, whales, seals, and their role in ocean ecosystems."),
        ("Bird Migration Secrets", "Kenn Kaufman", "Understand bird navigation, seasonal migration, nesting, and wildlife conservation."),
        ("Elephant Kingdom", "Cynthia Moss", "Explore elephant intelligence, memory, leadership, and family structures."),
        ("The Soul of an Octopus", "Sy Montgomery", "Learn about octopus intelligence, problem solving, and marine biology."),
        ("Wildlife Conservation", "Richard Leakey", "Protect endangered species and understand anti-poaching efforts worldwide."),
        ("Beekeeping Essentials", "Diana Sammataro", "Introduction to honeybees, pollination, and maintaining healthy bee colonies."),
        ("Big Cats of Africa", "Jonathan Scott", "Study lions, leopards, cheetahs, hunting tactics, and predator behavior."),

        // Business and Entrepreneurship Books
        ("The Lean Startup", "Eric Ries", "Build startups using MVPs, customer feedback, product validation, and rapid iteration."),
        ("Startup Funding Guide", "Alex Morgan", "Learn venture capital, angel investors, fundraising, pitching, and startup finance."),
        ("Business Strategy Masterclass", "Michael Porter", "Competitive advantage, market positioning, and long-term business growth."),
        ("Leadership Habits", "James Clear", "Build productive teams through habits, accountability, and organizational culture."),
        ("Marketing Psychology", "Robert Cialdini", "Consumer behavior, persuasion, branding, and customer decision making."),
        ("Scaling Companies", "Ben Horowitz", "Managing growth, hiring executives, and overcoming startup challenges."),
        ("Negotiation Secrets", "Chris Voss", "Improve communication, conflict resolution, and business negotiations."),
        ("Personal Finance for Entrepreneurs", "Josh Kaufman", "Cash flow, budgeting, investing, and financial planning for founders."),
        ("Blue Ocean Strategy", "W. Chan Kim", "Create new markets and avoid competition through innovation."),
        ("Thinking Fast and Slow", "Daniel Kahneman", "Behavioral economics, decision making, and cognitive biases."),

        // Software Development Books
        ("Clean Code", "Robert C. Martin", "Write readable, maintainable software using clean architecture and coding principles."),
        ("ASP.NET Core Web APIs", "Andrew Lock", "Build REST APIs using ASP.NET Core, dependency injection, middleware, and Swagger."),
        ("C# in Depth", "Jon Skeet", "Master LINQ, async programming, generics, and advanced C# features."),
        ("Entity Framework Core", "Julie Lerman", "Database access, migrations, LINQ queries, and performance optimization using EF Core."),
        ("Domain Driven Design", "Eric Evans", "Model complex business domains using aggregates, bounded contexts, and ubiquitous language."),
        ("Microservices in Practice", "Sam Newman", "Service communication, API gateways, distributed systems, and cloud deployment."),
        ("PostgreSQL Performance Tuning", "Regina Obe", "Indexes, query optimization, execution plans, and database scalability."),
        ("Design Patterns in C#", "Eric Freeman", "Factory, Strategy, Repository, and other software design patterns."),
        ("Building Cloud Native Applications", "Mark Richards", "Docker, Kubernetes, observability, resilience, and cloud architecture."),
        ("Semantic Search with AI", "Martin Kleppmann", "Vector databases, embeddings, retrieval systems, RAG, pgvector, and AI-powered search.")
    ];

    public static async Task SeedAsync(BookStoreDbContext db, IEmbeddingService embeddingService, CancellationToken cancellationToken = default)
    {
        if (await db.Books.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var (title, author, description) in SeedBooks)
        {
            var embedding = await embeddingService.GenerateBookEmbeddingAsync(title, description, author, cancellationToken);

            db.Books.Add(new Book
            {
                Title = title,
                Author = author,
                Description = description,
                Embedding = embedding,
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
