using Microsoft.Playwright;

public class IMDBProvider : IActorProvider
{
    private readonly IPlaywrightService _playwrightService;
    private readonly ILogger<IMDBProvider> _logger;

    public IMDBProvider(IPlaywrightService playwrightService, ILogger<IMDBProvider> logger)
    {
        _playwrightService = playwrightService;
        _logger = logger;
    }

    public async Task<List<ActorModel>> ScrapeActorsAsync()
    {
        var actors = new List<ActorModel>();
        var page = await _playwrightService.GetPageAsync("https://www.imdb.com/chart/top/");

        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var movieNodes = await page.QuerySelectorAllAsync("li[class*='ipc-metadata-list-summary-item']");

            if (movieNodes.Count == 0)
            {
                _logger.LogWarning("No movie nodes found on the IMDB page.");
                return actors;
            }

            foreach (var node in movieNodes)
            {
                try
                {
                    var movieId = await ExtractMovieIdAsync(node);
                    var (title, rank) = await ExtractTitleAndRankAsync(node);
                    var year = await ExtractYearAsync(node);
                    var rating = await ExtractRatingAsync(node);

                    actors.Add(new ActorModel
                    {
                        Id = movieId,
                        Name = title,
                        Rank = rank,
                        Details = $"Year: {year}, Rating: {rating}",
                        Type = "Movie",
                        Source = "IMDB"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing IMDB node.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scrape IMDB.");
        }

        return actors;
    }

    private async Task<string?> ExtractMovieIdAsync(IElementHandle node)
    {
        var detailsNode = await node.QuerySelectorAsync("a.ipc-title-link-wrapper");
        var href = await detailsNode?.GetAttributeAsync("href");

        return href?.Split('/')[2]; // Extract ID
    }

    private async Task<(string? Title, int Rank)> ExtractTitleAndRankAsync(IElementHandle node)
    {
        var detailsNode = await node.QuerySelectorAsync("a.ipc-title-link-wrapper");
        var titleNode = await detailsNode?.QuerySelectorAsync("h3.ipc-title__text");
        var fullTitle = await titleNode?.InnerTextAsync();
        var rank = int.Parse(fullTitle?.Split('.')[0].Trim() ?? "0");
        var title = fullTitle?.Substring(fullTitle.IndexOf(' ') + 1).Trim();

        return (title, rank);
    }

    private async Task<string?> ExtractYearAsync(IElementHandle node)
    {
        var yearNode = await node.QuerySelectorAsync("span[class*='cli-title-metadata-item']");

        return await yearNode?.InnerTextAsync();
    }

    private async Task<string?> ExtractRatingAsync(IElementHandle node)
    {
        var ratingNode = await node.QuerySelectorAsync("span.ipc-rating-star--rating");

        return await ratingNode?.InnerTextAsync();
    }
}
