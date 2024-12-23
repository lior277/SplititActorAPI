using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Splitit.ActorAPI.Web.ActorApi.Extensions;
using Splitit.ActorAPI.Web.ActorApi.Filter;
using Splitit.ActorAPI.Web.ActorApi.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.UseKestrel()
    .ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5171); // HTTP
        options.ListenAnyIP(7262, listenOptions => listenOptions.UseHttps()); // HTTPS
    });

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ActorsDb"));

// Register application services using extension method
builder.Services.AddApplicationServices(builder.Configuration);

// Add Swagger and API Explorer for API documentation
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Splitit.ActorAPI",
        Version = "1.0.0",
        Description = "API for managing and scraping actor information from various sources."
    });

    // Add Bearer token security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token in the text input below.\n\nExample: `Bearer 12345abcdef`"
    });

    // Register the operation filter
    options.OperationFilter<AuthorizeCheckOperationFilter>();
});



// Add custom Bearer authentication scheme
builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, BearerTokenAuthenticationHandler>("Bearer", options => { });

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Splitit.ActorAPI v1");
        options.DefaultModelsExpandDepth(-1); // Disable model expansion
    });
}

app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Create a logger instance and log service status after app is built
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Service is up and running on URLs: {Urls}", app.Urls);
}

app.Run();
