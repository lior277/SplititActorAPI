using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Splitit.ActorAPI.Web.ActorApi.Extensions;
using Splitit.ActorAPI.Web.ActorApi.Handler;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Kestrel
builder.WebHost.UseKestrel()
    .ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5171); // HTTP
        options.ListenAnyIP(7262, listenOptions => listenOptions.UseHttps()); // HTTPS
    });

// Configure In-Memory Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ActorsDb"));

// Register custom application services
builder.Services.AddApplicationServices(builder.Configuration);

// Configure Swagger with Bearer token support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Splitit.ActorAPI",
        Version = "1.0.0",
        Description = "API for managing and scraping actor information."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",  // Change to custom format if required
        Description = "Enter 'Bearer' followed by your token (e.g., Bearer abc123)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                Name = "Authorization",
                In = ParameterLocation.Header
            },
            Array.Empty<string>() // No specific scopes required
        }
    });
});

// Add custom Bearer authentication (skipping JWT validation)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddScheme<AuthenticationSchemeOptions, CustomBearerAuthenticationHandler>("Bearer", options => { });

// Register HttpClient for dependency injection
builder.Services.AddHttpClient();

// Add other application services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Seed data asynchronously during app startup
using (var scope = app.Services.CreateScope())
{
    var actorRepository = scope.ServiceProvider.GetRequiredService<IActorRepository>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Application is starting...");

    await actorRepository.SeedDataAsync();
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
