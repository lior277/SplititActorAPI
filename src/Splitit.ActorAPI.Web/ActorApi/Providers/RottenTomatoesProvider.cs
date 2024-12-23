﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

public class RottenTomatoesProvider : IActorProvider
{
    private readonly IPlaywrightService _playwrightService;
    private readonly ILogger<RottenTomatoesProvider> _logger;

    public RottenTomatoesProvider(IPlaywrightService playwrightService, ILogger<RottenTomatoesProvider> logger)
    {
        _playwrightService = playwrightService;
        _logger = logger;
    }

    public async Task<List<ActorModel>> ScrapeActorsAsync()
    {
        var actors = new List<ActorModel>();
        const string baseUrl = "https://editorial.rottentomatoes.com/guide/best-movies-of-all-time/";

        try
        {
            var page = await _playwrightService.GetPageAsync(baseUrl);

            while (true)
            {
                var movieNodes = await page.QuerySelectorAllAsync(".countdown-item .article_movie_title");

                if (movieNodes.Count == 0)
                {
                    _logger.LogWarning("No movie nodes found on this page.");
                    break;
                }

                foreach (var node in movieNodes)
                {
                    try
                    {
                        var rankText = await node.EvaluateAsync<string>(
                            "el => el.closest('.countdown-item').querySelector('.countdown-index').textContent");
                        var rank = int.Parse(rankText.Trim().Replace("#", string.Empty));

                        var titleNode = await node.QuerySelectorAsync("h2 a");
                        var titleText = await titleNode?.InnerTextAsync() ?? "Unknown Title";

                        var yearNode = await node.QuerySelectorAsync("h2 span.subtle.start-year");
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

                var nextButtons = await page.QuerySelectorAllAsync("a.post-page-numbers");
                var navigatedToNextPage = false;

                foreach (var button in nextButtons)
                {
                    var text = (await button.InnerTextAsync()).Trim();
                    if (text.Equals("Next", StringComparison.OrdinalIgnoreCase))
                    {
                        var nextUrl = await button.GetAttributeAsync("href");
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
