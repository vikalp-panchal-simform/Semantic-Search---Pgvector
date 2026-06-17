using System.ComponentModel.DataAnnotations;

namespace BookStore.Api.Models.Dtos;

public class CreateBookRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;
}
