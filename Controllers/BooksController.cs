using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
   private readonly LibraryContext _context;
   public BooksController(LibraryContext context) => _context = context;
   
   [HttpGet]
   public ActionResult<IEnumerable<Book>> GetBooks() => _context.Books.ToList();

   [HttpGet("{id}")]
   public ActionResult<Book> GetBooks(int id)
   {
      var book = _context.Books.Find(id);
      return book == null ? NotFound() : Ok(book);
   }

   [HttpPost]
   public ActionResult<Book> CreateBook(Book book)
   {
      if(!ModelState.IsValid) return BadRequest(ModelState);
      _context.Books.Add(book);
      _context.SaveChanges();
      return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
   }

   [HttpPut("{id}")]
   public IActionResult Update(int id, Book book)
   {
      if (id != book.Id) return BadRequest();
      if(!_context.Books.Any(b => b.Id == id)) return NotFound();
      _context.Entry(book).State = EntityState.Modified;
      _context.SaveChanges();
      return NoContent();
   }

   [HttpDelete("{id}")]
   public IActionResult Delete(int id)
   {
      var book = _context.Books.Find(id);
      if (book == null) return NotFound();
      _context.Books.Remove(book);
      _context.SaveChanges();
      return NoContent();
   }

   
}