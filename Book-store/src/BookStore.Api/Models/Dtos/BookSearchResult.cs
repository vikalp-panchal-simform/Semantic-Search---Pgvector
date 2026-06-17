namespace BookStore.Api.Models.Dtos;

public class BookSearchResult
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public double Similarity { get; set; }
}
