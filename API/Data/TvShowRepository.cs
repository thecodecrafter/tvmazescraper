using API.DTOs;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class TvShowRepository : ITvShowRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public TvShowRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TvShowDto>> GetTvShowsAsync(
            int skip,
            int take,
            CancellationToken cancellationToken
        )
        {
            var query = _context
                .TvShows.ProjectTo<TvShowDto>(_mapper.ConfigurationProvider)
                .AsNoTracking();

            return await query.Skip(skip).Take(take).ToListAsync();
        }
    }
}
