using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Data;
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
    public ActionResult<IEnumerable<Reservation>> GetReservations() => _context.Reservations.ToList();
    
    [HttpGet("{id}")]
    public ActionResult<Reservation> GetReservation(int id)
    {
        var reservation = _context.Reservations.Find(id);
        return reservation == null ? NotFound() : Ok(reservation);
    }
    
    [HttpPost]
    public ActionResult<Reservation> CreateReservation(Reservation reservation)
    {
        var book = _context.Books.Find(reservation.BookId);
        var user = _context.Users.Find(reservation.UserId);
        if (book == null || user == null) return NotFound("Book or User not found.");
        if (!book.isAvailable) return BadRequest("Book is not available.");
        
        book.isAvailable = false;
        _context.Reservations.Add(reservation);
        _context.SaveChanges();
        
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }
    
    [HttpPut("{id}")]
    public IActionResult Update(int id, Reservation reservation)
    {
        if (id != reservation.Id) return BadRequest();
        if(!_context.Reservations.Any(r => r.Id == id)) return NotFound();
        _context.Entry(reservation).State = EntityState.Modified;
        _context.SaveChanges();
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var reservation = _context.Reservations.Find(id);
        if (reservation == null) return NotFound();
        _context.Reservations.Remove(reservation);
        _context.SaveChanges();
        return NoContent();
    }
}