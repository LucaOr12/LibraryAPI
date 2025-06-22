namespace LibraryAPI.Models;
using System.ComponentModel.DataAnnotations;
public class Book
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = null!;
    [Required]
    public string Author { get; set; } = null!;

    public bool isAvailable { get; set; } = true;
}