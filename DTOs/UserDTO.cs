using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs;

public class UserDTO
{
    [Required]
    public string Name { get; set; }
    [EmailAddress]
    public string Email { get; set; }
}