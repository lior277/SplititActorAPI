namespace Splitit.ActorAPI.Web.ActorApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this
            IServiceCollection services, IConfiguration configuration)
        {
            var imdbUrl = configuration.GetValue<string>("IMDBProvider");
            var rottenTomatoesUrl = configuration.GetValue<string>("RottenTomatoesProvider");

            services.AddScoped<IActorProvider>(provider =>
            {
                var playwrightService = provider.GetRequiredService<IPlaywrightService>();
                var logger = provider.GetRequiredService<ILogger<IMDBProvider>>();
                return new IMDBProvider(playwrightService, logger, imdbUrl);
            });

            services.AddScoped<IActorProvider>(provider =>
            {
                var playwrightService = provider.GetRequiredService<IPlaywrightService>();
                var logger = provider.GetRequiredService<ILogger<RottenTomatoesProvider>>();
                return new RottenTomatoesProvider(playwrightService, logger, rottenTomatoesUrl);
            });

            services.AddScoped<IActorRepository, ActorRepository>();
            services.AddSingleton<IPlaywrightService, PlaywrightService>();

            return services;
        }
    }
}
