

public class RottenTomatoesProvider : IActorProvider
{
    private readonly IPlaywrightService _playwrightService;
    private readonly ILogger<RottenTomatoesProvider> _logger;
    private readonly string _rottenTomatoesUrl;
    private const string _movieTitle = ".countdown-item .article_movie_title";
    private const string _rankText = ".countdown-item .article_movie_title";
    private const string _titleNode = "h2 a";
    private const string _yearNode = "h2 span.subtle.start-year";
    private const string _nextButtons = "a.post-page-numbers";
    private const string _nextUrl = "href";

    public RottenTomatoesProvider(IPlaywrightService playwrightService,
        ILogger<RottenTomatoesProvider> logger, string rottenTomatoesUrl)
    {
        _playwrightService = playwrightService;
        _logger = logger;
        _rottenTomatoesUrl = rottenTomatoesUrl;
    }

    public async Task<List<ActorModel>> ScrapeActorsAsync()
    {
        var actors = new List<ActorModel>();

        try
        {
            var page = await _playwrightService.GetPageAsync(_rottenTomatoesUrl);

            while (true)
            {
                var movieNodes = await page.QuerySelectorAllAsync(_movieTitle);

                if (movieNodes.Count == 0)
                {
                    _logger.LogWarning("No movie nodes found on this page.");
                    break;
                }

                foreach (var node in movieNodes)
                {
                    try
                    {
                        var rankText = await node.EvaluateAsync<string>(_rankText);
                        var rank = int.Parse(rankText.Trim().Replace("#", string.Empty));

                        var titleNode = await node.QuerySelectorAsync(_titleNode);
                        var titleText = await titleNode?.InnerTextAsync() ?? "Unknown Title";

                        var yearNode = await node.QuerySelectorAsync(_yearNode);
                        var year = await yearNode?.InnerTextAsync();

                        actors.Add(new ActorModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = titleText,
                            Rank = rank,
                            Details = $"Year: {year}",
                            Type = "Movie",
                            Source = "Rotten Tomatoes"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing Rotten Tomatoes node.");
                    }
                }

                var nextButtons = await page.QuerySelectorAllAsync(_nextButtons);
                var navigatedToNextPage = false;

                foreach (var button in nextButtons)
                {
                    var text = (await button.InnerTextAsync()).Trim();
                    if (text.Equals("Next", StringComparison.OrdinalIgnoreCase))
                    {
                        var nextUrl = await button.GetAttributeAsync(_nextUrl);

                        if (!string.IsNullOrEmpty(nextUrl))
                        {
                            _logger.LogInformation("Navigating to next page: {NextUrl}", nextUrl);
                            await page.GotoAsync(nextUrl);
                            navigatedToNextPage = true;
                        }
                        break;
                    }
                }

                if (!navigatedToNextPage)
                {
                    _logger.LogInformation("No more pages to scrape.");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scrape Rotten Tomatoes.");
        }

        return actors;
    }
}
