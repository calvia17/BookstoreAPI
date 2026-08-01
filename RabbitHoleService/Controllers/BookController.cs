using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Objects;
using RabbitHoleService.Services;

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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        [AllowAnonymous]
        [HttpGet]
        [OutputCache(PolicyName = "DynamicData", Tags = ["books:all"])]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks(CancellationToken cancellationToken)
        {
            // Compare the etag sent by the client with the latest last modified timestamp of the books.
            // Even if the server caches is cleared, the last modified timestamp from the database will be used to generate the latest etag.
            // If the client's etag matches the latest etag, return 304 Not Modified.

            // If server cache exists, the etags are compared by the OutputCache middleware, and this method will not be executed.
            // In case the server cache is cleared, this method will be executed and the etags will be compared here to return 304 Not Modified when they match.
            var lastModified = await this.bookService.GetMaxLastModifiedAsync(cancellationToken);
            var currentEtag = $"\"{lastModified.Ticks}\"";
            Response.Headers.ETag = currentEtag;
            if (Request.Headers.TryGetValue("If-None-Match", out var etag) && etag.Contains(currentEtag))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            var books = await this.bookService.GetAllAsync(cancellationToken);
            return Ok(books);
        }

        /// <summary>
        /// Gets a book.
        /// </summary>
        /// <param name="id">The book id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [OutputCache(PolicyName = "DynamicData", Tags = ["book:id"])]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDto>> GetBook([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var book = await this.bookService.GetAsync(id, cancellationToken);
                Response.Headers.ETag = $"\"{book.LastModified.Ticks}\"";
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        [AllowAnonymous]
        [HttpGet("search")]
        [OutputCache(PolicyName = "DynamicData", VaryByQueryKeys = ["Isbn", "Name", "Author", "MinimumCost", "MaximumCost", "Genres"], Tags = ["books:search"])]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<BookDto>>> FindBooks([FromQuery] BookSearchRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var books = await this.bookService.FindBooksAsync(request, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added book.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookDto>> AddBook([FromBody] CreateBookDto newBookData, CancellationToken cancellationToken)
        {
            try
            {
                var createdBook = await this.bookService.CreateAsync(newBookData, cancellationToken);
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
                    Title = "Book Already Exists",
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added books.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<IEnumerable<BookDto>>> AddBooks([FromBody] IEnumerable<CreateBookDto> newBooksData, CancellationToken cancellationToken)
        {
            try
            {
                var createdBooks = await this.bookService.CreateMultipleAsync(newBooksData, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook([FromRoute] Guid id, [FromBody] UpdateBookDto updateData, CancellationToken cancellationToken)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                if (updateData.Cost.HasValue && !isAdmin)
                {
                    return Forbid();
                }

                await this.bookService.UpdateAsync(id, updateData, cancellationToken);
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
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPatch("bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBooks([FromBody] UpdateMultipleBooksDto updateData, CancellationToken cancellationToken)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                if (updateData.Books.Select(x => x.UpdateData.Cost).Any(cost => cost.HasValue) && !isAdmin)
                {
                    return Forbid();
                }

                await this.bookService.UpdateMultipleAsync(updateData, cancellationToken);
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
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteBook([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await this.bookService.DeleteAsync(id, cancellationToken);
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