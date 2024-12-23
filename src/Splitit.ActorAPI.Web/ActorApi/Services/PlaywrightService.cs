using Microsoft.Playwright;

public class PlaywrightService : IPlaywrightService
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;

    public async Task<IPage> GetPageAsync(string url)
    {
        if (_playwright == null)
        {
            _playwright = await Playwright.CreateAsync();
        }

        if (_browser == null)
        {
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--start-maximized", "--disable-blink-features=AutomationControlled" },
                IgnoreDefaultArgs = new[] { "--enable-automation" },
            });
        }

        if (_context == null)
        {
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = null,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
            });
        }

        var page = await _context.NewPageAsync();
        await page.GotoAsync(url);

        return page;
    }

    public async Task CloseBrowserAsync()
    {
        if (_context != null)
        {
            await _context.CloseAsync();
            _context = null;
        }

        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        if (_playwright != null)
        {
            _playwright.Dispose();
            _playwright = null;
        }
    }
}
