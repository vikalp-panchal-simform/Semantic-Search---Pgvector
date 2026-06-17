using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace BookStore.Api.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    [Column(TypeName = "vector(384)")]
    public Vector? Embedding { get; set; }

    public DateTime CreatedAt { get; set; }
}
