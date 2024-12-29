using Microsoft.Playwright;

public class IMDBProvider : IActorProvider
{
    private readonly IPlaywrightService _playwrightService;
    private readonly ILogger<IMDBProvider> _logger;
    private readonly string _imdbUrl;
    private const string _movieNodes = "li[class*='ipc-metadata-list-summary-item']";
    private readonly string _detailsNode = "a.ipc-title-link-wrapper";
    private readonly string _titleNode = "h3.ipc-title__text";
    private readonly string _yearNode = "span[class*='cli-title-metadata-item']";
    private readonly string _ratingNode = "span.ipc-rating-star--rating";

    public IMDBProvider(IPlaywrightService playwrightService,
        ILogger<IMDBProvider> logger, string imdbUrl)
    {
        _playwrightService = playwrightService;
        _logger = logger;
        _imdbUrl = imdbUrl;
    }

    public async Task<List<ActorModel>> ScrapeActorsAsync()
    {
        var actors = new List<ActorModel>();
        var page = await _playwrightService.GetPageAsync(_imdbUrl);

        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var movieNodes = await page.QuerySelectorAllAsync(_movieNodes);

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
        var detailsNode = await node.QuerySelectorAsync(_detailsNode);
        var href = await detailsNode?.GetAttributeAsync("href");

        return href?.Split('/')[2]; // Extract ID
    }

    private async Task<(string? Title, int Rank)> ExtractTitleAndRankAsync(IElementHandle node)
    {
        var detailsNode = await node.QuerySelectorAsync(_detailsNode);
        var titleNode = await detailsNode?.QuerySelectorAsync(_titleNode);
        var fullTitle = await titleNode?.InnerTextAsync();
        var rank = int.Parse(fullTitle?.Split('.')[0].Trim() ?? "0");
        var title = fullTitle?.Substring(fullTitle.IndexOf(' ') + 1).Trim();

        return (title, rank);
    }

    private async Task<string?> ExtractYearAsync(IElementHandle node)
    {
        var yearNode = await node.QuerySelectorAsync(_yearNode);

        return await yearNode?.InnerTextAsync();
    }

    private async Task<string?> ExtractRatingAsync(IElementHandle node)
    {
        var ratingNode = await node.QuerySelectorAsync(_ratingNode);

        return await ratingNode?.InnerTextAsync();
    }
}
