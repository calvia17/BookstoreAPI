using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Storage;
using NuGet.Protocol.Core.Types;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Objects;
using RabbitHoleService.Services;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RabbitHoleService.Controllers
{
    /// <summary>
    /// The book controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService bookService;

        /// <summary>
        /// Initialises the book controller.
        /// </summary>
        /// <param name="bookService">The book service.</param>
        public BookController(IBookService bookService)
        {
            this.bookService = bookService;
        }

        /// <summary>
        /// Gets all the books.
        /// </summary>
        /// <returns>The books.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
        {
            var books = await this.bookService.GetAllAsync();
            return Ok(books);
        }

        /// <summary>
        /// Gets a book.
        /// </summary>
        /// <param name="id">The book id.</param>
        /// <returns>The book.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDto>> GetBook([FromRoute] Guid id)
        {
            try
            {
                var book = await this.bookService.GetAsync(id);
                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (BookNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Book Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    BookIds = ex.BookIds
                });
            }
        }

        /// <summary>
        /// Finds books that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The books.</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<BookDto>>> FindBooks([FromQuery] BookSearchRequestDto request)
        {
            try
            {
                var books = await this.bookService.FindBooksAsync(request);
                return Ok(books);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a book.
        /// </summary>
        /// <param name="newBookData">The new book data.</param>
        /// <returns>The added book.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookDto>> AddBook([FromBody] CreateBookDto newBookData)
        {
            try
            {
                var createdBook = await this.bookService.CreateAsync(newBookData);
                return CreatedAtAction(nameof(this.GetBook), new { id = createdBook.Id }, createdBook);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (BookAlreadyExistsException ex)
            {
                return Conflict(new
                {
                    Title = "Books Already Exist",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                    ExistingBooks = ex.ExistingBooks
                });
            }
        }

        /// <summary>
        /// Adds multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        /// <returns>The added books.</returns>
        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<IEnumerable<BookDto>>> AddBooks([FromBody] IEnumerable<CreateBookDto> newBooksData)
        {
            try
            {
                var createdBooks = await this.bookService.CreateMultipleAsync(newBooksData);
                return CreatedAtAction(nameof(this.GetAllBooks), createdBooks);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DuplicateBookInputException ex)
            {
                return Conflict(new
                {
                    Title = "Duplicate Books",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message,
                    DuplicateIsbns = ex.DuplicateIsbns
                });
            }
            catch (BookAlreadyExistsException ex)
            {
                return Conflict(new
                {
                    Title = "Books Already Exist",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                    ExistingBooks = ex.ExistingBooks
                });
            }
        }

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="id">The book id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <returns>No content.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook([FromRoute] Guid id, [FromBody] UpdateBookDto updateData)
        {
            try
            {
                await this.bookService.UpdateAsync(id, updateData);
                return NoContent();
            }
            catch (BookNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Book Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    BookIds = ex.BookIds
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates multiple books.
        /// </summary>
        /// <param name="updateData">The data to update.</param>
        /// <returns>No content.</returns>
        [HttpPatch("bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBooks([FromBody] UpdateMultipleBooksDto updateData)
        {
            try
            {
                await this.bookService.UpdateMultipleAsync(updateData);
                return NoContent();
            }
            catch (DuplicateBookInputException ex)
            {
                return Conflict(new
                {
                    Title = "Duplicate Books",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                    DuplicateIds = ex.DuplicateIds
                });
            }
            catch (BookNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Books Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    BookIds = ex.BookIds
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">The book id.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteBook([FromRoute] Guid id)
        {
            try
            {
                await this.bookService.DeleteAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (BookNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Book Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    BookIds = ex.BookIds
                });
            }
        }
    }
}