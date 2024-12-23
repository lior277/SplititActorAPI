
public class ActorRepository : IActorRepository
{
    private readonly AppDbContext _context;
    private readonly IEnumerable<IActorProvider> _actorProviders;
    private readonly ILogger<ActorRepository> _logger;

    public ActorRepository(AppDbContext context,
        IEnumerable<IActorProvider> actorProviders, ILogger<ActorRepository> logger)
    {
        _context = context;
        _actorProviders = actorProviders;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        try
        {
            foreach (var provider in _actorProviders)
            {
                var actors = await provider.ScrapeActorsAsync();

                if (actors != null && actors.Any())
                {
                    var newActors = actors.Where(actor =>
                    !_context.Actors.Any(existing => existing.Id == actor.Id));

                    if (newActors.Any())
                    {
                        _context.Actors.AddRange(newActors);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"{newActors.Count()}" +
                            $" actors seeded from {provider.GetType().Name}.");
                    }
                    else
                    {
                        _logger.LogInformation($"No new actors to seed from {provider.GetType().Name}.");
                    }
                }
                else
                {
                    _logger.LogWarning($"No actors scraped from {provider.GetType().Name}. Seed operation aborted.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during seed operation: {ex.Message}", ex);

            throw;
        }
    }
}
