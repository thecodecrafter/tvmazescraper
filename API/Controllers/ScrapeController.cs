using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrapeController : ControllerBase
    {
        private readonly TvMazeService _service;

        public ScrapeController(TvMazeService _service)
        {
            this._service = _service;
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunScraper(CancellationToken cancellationToken)
        {
            await _service.ScrapeTvShowsAsync(cancellationToken);
            return Ok("Scraping completed");
        }
    }
}
