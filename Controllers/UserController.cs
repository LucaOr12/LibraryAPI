using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using LibraryAPI.Services;

namespace LibraryAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class UserController : ControllerBase
{
    private readonly LibraryContext _context;
    private readonly IjwtService _jwtService;

    public UserController(IjwtService jwtService, LibraryContext context)
    {
        _jwtService = jwtService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDTO>>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();

        var usersDto = users.Select(u => new UserDTO
        {
            Name = u.Name,
            Email = u.Email
        }).ToList();
        return Ok(usersDto);
    }
    
    [HttpGet("{id}", Name = "GetUser")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        var userDto = new UserDTO
        {
            Name = user.Name,
            Email = user.Email
        };
        return Ok(userDto);
    }
    
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UserDTO>> RegisterUser(UserRegisterDTO registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            return BadRequest("Email is already taken");
        }

        var user = new User
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateJwtToken(user);

        var userDto = new UserDTO
        {
            Name = user.Name,
            Email = user.Email
        };
        return Ok(new {user = userDto, token});
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] UserLoginDTO loginDto)
    {
        if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            return BadRequest("Email and password are required");

        var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (userFromDb == null) return NotFound("User not found");
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, userFromDb.Password)) return Unauthorized("Invalid password");

        var token = _jwtService.GenerateJwtToken(userFromDb);

        var userDto = new UserDTO
        {
            Name = userFromDb.Name,
            Email = userFromDb.Email
        };
        return Ok(new { token, user = userDto });
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<object>> RefreshToken()
    {
        var userIdClaim = User.FindFirst("userId");
        if(userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var newToken = _jwtService.GenerateJwtToken(user);

        var userDto = new UserDTO
        {
            Name = user.Name,
            Email = user.Email
        };

        return Ok(new { token = newToken, user = userDto });
    }
    
    [HttpPut("{id}")]
    public async Task <IActionResult> Update(int id, UserDTO userDto)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id);
        if (!emailExists) return BadRequest("Email is already taken");
        var exist = await _context.Users.FindAsync(id);
        if(exist == null) return NotFound();
        exist.Name = userDto.Name;
        exist.Email = userDto.Email;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if(existingUser == null) return NotFound();
        _context.Users.Remove(existingUser);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}