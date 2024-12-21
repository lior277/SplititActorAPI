using Microsoft.AspNetCore.Mvc;
using System.Linq;

[ApiController]
[Route("api/v1/actors")]
public class ActorsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActorsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Retrieve a list of all actors with filtering and pagination
    // GET: api/v1/actors
    [HttpGet]
    public IActionResult GetActors(
        [FromQuery] string? name,
        [FromQuery] int? minRank,
        [FromQuery] int? maxRank,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Page and pageSize must be greater than 0.");
        }

        var query = _context.Actors.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(a => a.Name.Contains(name));
        }

        if (minRank.HasValue)
        {
            query = query.Where(a => a.Rank >= minRank.Value);
        }

        if (maxRank.HasValue)
        {
            query = query.Where(a => a.Rank <= maxRank.Value);
        }

        // Pagination
        var totalActors = query.Count();
        var totalPages = (int)Math.Ceiling(totalActors / (double)pageSize);
        var actors = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new { a.Id, a.Name }) // Only return Id and Name
            .ToList();

        var result = new
        {
            TotalActors = totalActors,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize,
            Actors = actors
        };

        return Ok(result);
    }

    // 2. Retrieve details for a specific actor
    // GET: api/v1/actors/{id}
    [HttpGet("{id}")]
    public IActionResult GetActor(string id)
    {
        var actor = _context.Actors.Find(id);
        if (actor == null) return NotFound("Actor not found.");

        return Ok(actor);
    }

    // 3. Add a new actor
    // POST: api/v1/actors
    [HttpPost]
    public IActionResult AddActor([FromBody] ActorModel actor)
    {
        if (actor == null) return BadRequest("Actor data is required.");

        if (_context.Actors.Any(a => a.Rank == actor.Rank))
        {
            return BadRequest("Duplicate rank not allowed.");
        }

        _context.Actors.Add(actor);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetActor), new { id = actor.Id }, actor);
    }

    // 4. Update an existing actor
    // PUT: api/v1/actors/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateActor(string id, [FromBody] ActorModel updatedActor)
    {
        if (updatedActor == null) return BadRequest("Actor data is required.");

        var actor = _context.Actors.Find(id);
        if (actor == null) return NotFound("Actor not found.");

        if (_context.Actors.Any(a => a.Rank == updatedActor.Rank && a.Id != id))
        {
            return BadRequest("Duplicate rank not allowed.");
        }

        // Update actor details
        actor.Name = updatedActor.Name;
        actor.Details = updatedActor.Details;
        actor.Type = updatedActor.Type;
        actor.Rank = updatedActor.Rank;
        actor.Source = updatedActor.Source;

        _context.SaveChanges();
        return NoContent();
    }

    // 5. Delete an actor
    // DELETE: api/v1/actors/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteActor(string id)
    {
        var actor = _context.Actors.Find(id);
        if (actor == null) return NotFound("Actor not found.");

        _context.Actors.Remove(actor);
        _context.SaveChanges();
        return NoContent();
    }
}
