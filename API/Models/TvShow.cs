namespace API.Models
{
    public class TvShow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<CastMember> CastMembers { get; set; } = [];
    }
}
