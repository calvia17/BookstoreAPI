using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Objects;
using RabbitHoleService.Services;

namespace RabbitHoleService.Controllers
{
    /// <summary>
    /// The genre controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService genreService;

        /// <summary>
        /// Initialises the genre controller.
        /// </summary>
        /// <param name="genreService">The genre service..</param>
        public GenreController(IGenreService genreService)
        {
            this.genreService = genreService;
        }

        /// <summary>
        /// Gets all the genres.
        /// </summary>
        /// <returns>The genres.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreDto>>> GetAllGenres()
        {
            var genres = await this.genreService.GetAllAsync();
            return Ok(genres);
        }
    }
}
