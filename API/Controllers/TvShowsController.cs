using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TvShowsController : ControllerBase
    {
        private const int PAGE_SIZE = 10;
        private readonly ITvShowRepository _tvShowRepository;

        public TvShowsController(ITvShowRepository tvShowRepository)
        {
            _tvShowRepository = tvShowRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<TvShowDto>>> GetShows(
            int? page,
            CancellationToken cancellationToken
        )
        {
            int pageNumber = page ?? 1;
            int skip = (pageNumber - 1) * PAGE_SIZE;
            var tvShows = await _tvShowRepository.GetTvShowsAsync(
                skip,
                PAGE_SIZE,
                cancellationToken
            );

            return Ok(tvShows.ToList());
        }
    }
}
