using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class IMDBProvider
{
    public HtmlWeb CreateHtmlWebClient()
    {
        return new HtmlWeb
        {
            PreRequest = request =>
            {
                request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
                request.Timeout = 100000; // Set timeout to 100 seconds
                return true;
            }
        };
    }

    private async Task<HtmlDocument> LoadDocumentWithRetriesAsync(string url, int maxRetries = 3)
    {
        var web = CreateHtmlWebClient();
        int retries = 0;

        while (true)
        {
            try
            {
                return await web.LoadFromWebAsync(url);
            }
            catch (Exception ex) when (retries < maxRetries)
            {
                retries++;
                Console.WriteLine($"Retry {retries}/{maxRetries}: {ex.Message}");
                await Task.Delay(1000); // Wait before retrying
            }
            catch
            {
                throw; // Re-throw after max retries
            }
        }
    }


    public async Task<List<ActorModel>> ScrapeActorsAsync()
    {
        var actors = new List<ActorModel>();
        var web = CreateHtmlWebClient();
        var document = await LoadDocumentWithRetriesAsync("https://www.imdb.com/chart/top/");

        // XPath to select list items containing movie data
        var movieNodes = document.DocumentNode.SelectNodes("//li[contains(@class, 'ipc-metadata-list-summary-item')]");

        if (movieNodes == null)
        {
            Console.WriteLine("No movie nodes found on the page.");
            return actors;
        }

        foreach (var node in movieNodes)
        {
            try
            {
                // Extract movie ID from the href attribute
                var detailsNode = node.SelectSingleNode(".//a[contains(@class, 'ipc-title-link-wrapper')]");
                var href = detailsNode?.GetAttributeValue("href", "");
                var movieId = href?.Split('/')[2]; // Extract ID 

                // Extract title and rank
                var titleNode = detailsNode?.SelectSingleNode(".//h3[@class='ipc-title__text']");
                var fullTitle = titleNode?.InnerText.Trim();
                var rank = Convert.ToInt32(fullTitle?.Split('.')[0]); // Extract rank
                var title = fullTitle?.Substring(fullTitle.IndexOf(' ') + 1); // Extract title

                // Extract year
                var yearNode = node.SelectSingleNode(".//span[contains(@class, 'cli-title-metadata-item')]");
                var year = yearNode?.InnerText.Trim(); // e.g., "1994"

                // Extract rating
                var ratingNode = node.SelectSingleNode(".//span[contains(@class, 'ipc-rating-star--rating')]");
                var ratingText = ratingNode?.InnerText.Trim();

                actors.Add(new ActorModel
                {
                    Id = movieId,
                    Name = title,
                    Rank = rank,
                    Details = $"{year} {ratingText}",
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing node: {ex.Message}");
            }
        }

        return actors;
    }

    // Pagination method
    public List<ActorModel> GetPaginatedActors(List<ActorModel> actors, int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            throw new ArgumentException("Page and pageSize must be greater than 0.");
        }

        return actors
            .Skip((page - 1) * pageSize) // Skip records based on the current page
            .Take(pageSize)             // Take only the records for the requested page
            .ToList();
    }
}
