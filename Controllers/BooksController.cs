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
   
   public class OpenLibraryDoc
   {
      public int? cover_i { get; set; }
   }

   public class OpenLibraryResponse
   {
      public List<OpenLibraryDoc> docs { get; set; } = new();
   }
   
   private async Task<string?> FetchCoverFromOpenLibrary(string title, string author)
   {
      using var httpClient = new HttpClient();
      var query = $"https://openlibrary.org/search.json?title={Uri.EscapeDataString(title)}&author={Uri.EscapeDataString(author)}";

      try
      {
         var response = await httpClient.GetFromJsonAsync<OpenLibraryResponse>(query);
         var coverId = response?.docs?.FirstOrDefault(d => d.cover_i != null)?.cover_i;
         return coverId != null ? $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg" : null;
      }
      catch
      {
         return null;
      }
   }

   [HttpGet("cover")]
   public async Task<ActionResult<IEnumerable<Book>>> GetCovers(string title, string author)
   {
      using var httpClient = new HttpClient();
      var query = $"https://openlibrary.org/search.json?title={Uri.EscapeDataString(title)}&author={Uri.EscapeDataString(author)}";
      
      try
      {
         var response = await httpClient.GetStringAsync(query);
         return Content(response, "application/json");
      }
      catch
      {
         return StatusCode(500, "Failed to fetch from OpenLibrary");
      }
   }

   [HttpPost]
   public async Task<ActionResult<Book>> CreateBook(Book book)
   {
      if (!ModelState.IsValid) return BadRequest(ModelState);

      // Fetch cover from OpenLibrary
      using var httpClient = new HttpClient();
      var query = $"https://openlibrary.org/search.json?title={Uri.EscapeDataString(book.Title)}&author={Uri.EscapeDataString(book.Author)}";

      try
      {
         var response = await httpClient.GetStringAsync(query);
         var json = System.Text.Json.JsonDocument.Parse(response);

         var coverId = json.RootElement
            .GetProperty("docs")
            .EnumerateArray()
            .FirstOrDefault(doc => doc.TryGetProperty("cover_i", out _))
            .GetProperty("cover_i")
            .GetInt32();

         book.CoverUrl = $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg";
      }
      catch
      {
         book.CoverUrl = null; // fallback
      }

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