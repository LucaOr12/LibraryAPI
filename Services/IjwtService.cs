using LibraryAPI.Models;

namespace LibraryAPI.Services;

public interface IjwtService
{
    string GenerateJwtToken(User user);
}