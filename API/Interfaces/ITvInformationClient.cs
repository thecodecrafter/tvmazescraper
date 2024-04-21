using API.Models;

namespace API.Interfaces
{
    public interface ITvClient
    {
        Task<IEnumerable<TvShow>> GetAllTvShowsAsync();
        Task<IEnumerable<CastMember>> GetCastAsync(int showId);
    }
}