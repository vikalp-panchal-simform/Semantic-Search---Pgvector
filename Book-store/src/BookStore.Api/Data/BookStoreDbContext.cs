using BookStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Data;

public class BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Would install the vector extension if it doesn't exist, but it requires superuser privileges
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(b => b.Title).HasMaxLength(500).IsRequired();
            entity.Property(b => b.Description).HasMaxLength(4000).IsRequired();
            entity.Property(b => b.Author).HasMaxLength(300).IsRequired();

            entity.HasIndex(b => b.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
        });
    }
}
