using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Splitit.ActorAPI.Web.ActorApi.Controllers
{
    [ApiController]
    [Route("api/v1/actors")]
    [Authorize] 
    public class ActorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ActorsController> _logger;
        private readonly IEnumerable<IActorProvider> _actorProviders;

        public ActorsController(AppDbContext context, ILogger<ActorsController>
            logger, IEnumerable<IActorProvider> actorProviders)
        {
            _context = context;
            _logger = logger;
            _actorProviders = actorProviders;
        }

        [HttpGet("scrape")]
        [Authorize]
        public async Task<IActionResult> ScrapeActors([FromQuery] string provider)
        {
            try
            {
                var selectedProvider = _actorProviders
                    .FirstOrDefault(p => p
                    .GetType()
                    .Name.Equals(provider, StringComparison.OrdinalIgnoreCase));

                if (selectedProvider == null)
                {
                    _logger.LogWarning("Invalid provider specified: {Provider}", provider);

                    return BadRequest(new
                    {
                        Message = "Invalid provider specified. Use" +
                        " 'IMDBProvider' or 'RottenTomatoesProvider'."
                    });
                }

                _logger.LogInformation("Scraping actors using provider: {Provider}", provider);

                var actors = await selectedProvider.ScrapeActorsAsync();

                _logger.LogInformation("Successfully scraped {Count}" +
                    " actors from {Provider}", actors.Count, provider);

                return Ok(actors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scraping actors from provider: {Provider}", provider);
                return StatusCode(500, "An error occurred while scraping data.");
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetActors([FromQuery] string? name,
            [FromQuery] int? minRank, [FromQuery] int? maxRank)
        {
            var query = _context.Actors.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(a => EF.Functions.Like(a.Name, $"%{name}%"));
            }

            if (minRank.HasValue)
            {
                query = query.Where(a => a.Rank >= minRank.Value);
            }

            if (maxRank.HasValue)
            {
                query = query.Where(a => a.Rank <= maxRank.Value);
            }

            var actors = query
                .OrderBy(a => a.Id)
                .Select(a => new { a.Name, a.Id })
                .ToList();

            return Ok(actors);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetActor(string id)
        {
            var actor = _context.Actors.Find(id);
            if (actor == null)
            {
                _logger.LogWarning("Actor with ID {Id} not found", id);
                return NotFound(new { Message = "Actor not found." });
            }

            return Ok(actor);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddActor([FromBody] ActorModel actor)
        {
            if (actor == null)
            {
                return BadRequest(new { Message = "Actor data is required." });
            }

            if (_context.Actors.Any(a => a.Rank == actor.Rank))
            {
                return BadRequest(new { Message = "Duplicate rank is not allowed." });
            }

            _context.Actors.Add(actor);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetActor), new { id = actor.Id }, actor);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult UpdateActor(string id, [FromBody] ActorModel updatedActor)
        {
            if (updatedActor == null)
            {
                return BadRequest("Actor data is required.");
            }

            var actor = _context.Actors.Find(id);

            if (actor == null)
            {
                return NotFound("Actor not found.");
            }

            if (_context.Actors.Any(a => a.Rank == updatedActor.Rank && a.Id != id))
            {
                return BadRequest("Duplicate rank not allowed.");
            }

            actor.Name = updatedActor.Name;
            actor.Details = updatedActor.Details;
            actor.Type = updatedActor.Type;
            actor.Rank = updatedActor.Rank;
            actor.Source = updatedActor.Source;

            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteActor(string id)
        {
            var actor = _context.Actors.Find(id);
            if (actor == null)
            {
                _logger.LogWarning("Actor with ID {Id} not found for deletion", id);
                return NotFound(new { Message = "Actor not found." });
            }

            _context.Actors.Remove(actor);
            _context.SaveChanges();
            return NoContent();
        }
    }
}