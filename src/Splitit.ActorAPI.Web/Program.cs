using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("ActorsDb"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed data asynchronously
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var imdbProvider = new IMDBProvider();

    try
    {
        // Scrape actors from IMDB
        var actors = await imdbProvider.ScrapeActorsAsync();

        // Ensure data is not empty
        if (actors == null || !actors.Any())
        {
            Console.WriteLine("No actors were scraped. Seed operation aborted.");
        }
        else
        {
            // Add scraped actors to the database
            context.Actors.AddRange(actors);
            await context.SaveChangesAsync();

            // Log seeded data
            Console.WriteLine("Actors seeded successfully:");
            foreach (var actor in context.Actors)
            {
                Console.WriteLine($"Id: {actor.Id}, Name: {actor.Name}, Rank: {actor.Rank}");
            }

            // Clear tracked entities to free memory
            context.ChangeTracker.Clear();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during seed operation: {ex.Message}");
        throw;
    }
}

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.Run();
