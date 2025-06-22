namespace LibraryAPI.Models;
using System.ComponentModel.DataAnnotations;
public class Reservation
{
    public int Id { get; set; }
    [Required]
    public int BookId { get; set; }
    [Required]
    public int UserId { get; set; }
    
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
}