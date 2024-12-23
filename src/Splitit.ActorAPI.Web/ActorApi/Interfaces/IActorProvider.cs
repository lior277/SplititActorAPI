public interface IActorProvider
{
    Task<List<ActorModel>> ScrapeActorsAsync();
}
