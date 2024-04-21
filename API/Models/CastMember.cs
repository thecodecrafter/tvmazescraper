namespace API.Models
{
    public class CastMember
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateOnly? Birthday { get; set; }
        public List<TvShow> TvShows { get; set; }
    }
}
