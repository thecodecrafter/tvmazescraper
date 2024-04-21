using API.DTOs;

namespace API.Interfaces
{
    public interface ITvShowRepository
    {
        Task<IEnumerable<TvShowDto>> GetTvShowsAsync(
            int skip,
            int take,
            CancellationToken cancellationToken
        );
    }
}
