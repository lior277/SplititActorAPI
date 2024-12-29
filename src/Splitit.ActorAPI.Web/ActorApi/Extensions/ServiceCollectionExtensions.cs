namespace Splitit.ActorAPI.Web.ActorApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this
            IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IActorRepository, ActorRepository>();
            services.AddScoped<IActorProvider, IMDBProvider>();
            services.AddScoped<IActorProvider, RottenTomatoesProvider>();
            services.AddSingleton<IPlaywrightService, PlaywrightService>();

            return services;
        }
    }
}
