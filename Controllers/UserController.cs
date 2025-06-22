using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UserController : ControllerBase
{
    private readonly LibraryContext _context;
    public UserController(LibraryContext context) => _context = context;
    
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers() => _context.Users.ToList();
    
    [HttpGet("{id}")]
    public ActionResult<User> GetUser(int id)
    {
        var user = _context.Users.Find(id);
        return user == null ? NotFound() : Ok(user);
    }
    
    [HttpPost]
    public ActionResult<User> CreateUser(User user)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        _context.Users.Add(user);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPost("login")]
    public ActionResult<User> Login([FromBody] JsonElement user)
    {
        if(!user.TryGetProperty("email", out var email) || !user.TryGetProperty("password", out var password)) return BadRequest();
        var userFromDb = _context.Users.FirstOrDefault(u => u.Email == email.GetString());
        if (userFromDb == null) return NotFound();
        if (!BCrypt.Net.BCrypt.Verify(password.GetString(), userFromDb.Password)) return Unauthorized();
        return Ok(userFromDb);
    }
    
    [HttpPut("{id}")]
    public IActionResult Update(int id, User user)
    {
        if (id != user.Id) return BadRequest();
        if(!_context.Users.Any(u => u.Id == id)) return NotFound();
        _context.Entry(user).State = EntityState.Modified;
        _context.SaveChanges();
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        _context.SaveChanges();
        return NoContent();
    }
}