namespace Splitit.ActorAPI.Web.ActorApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this
            IServiceCollection services, IConfiguration configuration)
        {
            // Add repositories
            services.AddScoped<IActorRepository, ActorRepository>();

            // Register providers as IActorProvider
            services.AddScoped<IActorProvider, IMDBProvider>();
            services.AddScoped<IActorProvider, RottenTomatoesProvider>();

            // Register PlaywrightService as a singleton
            services.AddSingleton<IPlaywrightService, PlaywrightService>();

            return services;
        }
    }
}
