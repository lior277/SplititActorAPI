
public interface IIMDBProvider
{
    Task<List<ActorModel>> ScrapeActorsAsync();
}