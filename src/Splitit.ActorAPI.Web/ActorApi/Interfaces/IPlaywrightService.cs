using Microsoft.Playwright;

public interface IPlaywrightService
{
    Task<IPage> GetPageAsync(string url);
}