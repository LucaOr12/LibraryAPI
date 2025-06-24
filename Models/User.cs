namespace LibraryAPI.Models;
using System.ComponentModel.DataAnnotations;
public class User
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}