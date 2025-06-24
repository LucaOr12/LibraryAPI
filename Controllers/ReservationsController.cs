using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ReservationsController : ControllerBase
{
    private readonly LibraryContext _context;
    public ReservationsController(LibraryContext context) => _context = context;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations() => await _context.Reservations.ToListAsync();
    
    [HttpGet("{id}")]
    public async Task <ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        return reservation == null ? NotFound() : Ok(reservation);
    }
    
    [HttpPost]
    public async Task<ActionResult<Reservation>> CreateReservation(ReservationCreateDTO dto)
    {
        var book = await _context.Books.FindAsync(dto.BookId);
        var user = await _context.Users.FindAsync(dto.UserId);
        if(book == null || user == null)
            return BadRequest("Invalid reservation");
        if (!book.isAvailable)
            return BadRequest("Book is not available");

        var reservation = new Reservation
        {
            BookId = dto.BookId,
            UserId = dto.UserId,
            ReservedAt = DateTime.UtcNow,
        };
        
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }
    
    [HttpPut("{id}")]
    public async Task <IActionResult> Update(int id, Reservation reservation)
    {
        if (id != reservation.Id) return BadRequest();
        var check = await _context.Reservations.AnyAsync(r => r.Id == id);
        if(!check) return NotFound();
        _context.Entry(reservation).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task <IActionResult> Delete(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();
        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}