public class ActorModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Details { get; set; }
    public string Type { get; set; } = "IMDB";
    public int Rank { get; set; }
    public string Source { get; set; } = "Movie";
}
