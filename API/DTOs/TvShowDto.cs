namespace API.DTOs
{
    public class TvShowDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<CastMemberDto> CastMembers { get; set; }
    }
}
