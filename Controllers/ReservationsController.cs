using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly LibraryContext _context;
    public ReservationsController(LibraryContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        => await _context.Reservations.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        return reservation == null ? NotFound() : Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<Reservation>> CreateReservation(Reservation incoming)
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized();

        var book = await _context.Books.FindAsync(incoming.BookId);
        if (book == null) return BadRequest("Invalid book");
        if (!book.isAvailable) return BadRequest("Book is not available");

        var reservation = new Reservation
        {
            BookId = incoming.BookId,
            UserId = userId,
            ReservedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Reservation reservation)
    {
        if (id != reservation.Id) return BadRequest();
        var exists = await _context.Reservations.AnyAsync(r => r.Id == id);
        if (!exists) return NotFound();

        _context.Entry(reservation).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}