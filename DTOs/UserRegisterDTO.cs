using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs;

public class UserRegisterDTO
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}