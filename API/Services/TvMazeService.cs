using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Extensions.Http;

namespace API.Services
{
    public class TvMazeService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;

        public TvMazeService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://api.tvmaze.com/");
            _serviceProvider = serviceProvider;
        }

        public async Task ScrapeTvShowsAsync(CancellationToken cancellationToken)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    }
                );

            var allShowIds = await GetAllShowIdsAsync(retryPolicy, cancellationToken);

            await Parallel.ForEachAsync(
                allShowIds,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 5,
                    CancellationToken = cancellationToken
                },
                async (showId, cancellationToken) =>
                {
                    using (var scope = _serviceProvider.CreateAsyncScope())
                    {
                        var scopedService =
                            scope.ServiceProvider.GetRequiredService<TvMazeService>();
                        await scopedService.ScrapeTvShowAsync(
                            showId,
                            retryPolicy,
                            cancellationToken
                        );
                    }
                }
            );
        }

        private async Task<List<int>> GetAllShowIdsAsync(
            IAsyncPolicy<HttpResponseMessage> retryPolicy,
            CancellationToken cancellationToken
        )
        {
            var showIds = new List<int>();
            int page = 0;
            bool morePages = true;

            while (morePages)
            {
                var response = await retryPolicy.ExecuteAsync(
                    () => _httpClient.GetAsync($"shows?page={page}", cancellationToken)
                );
                if (!response.IsSuccessStatusCode)
                    break;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var shows = JsonConvert.DeserializeObject<List<TvShow>>(content);

                if (shows.Any())
                {
                    showIds.AddRange(shows.Select(s => s.Id));
                    page++;
                }
                else
                {
                    morePages = false;
                }
            }

            return showIds;
        }

        private async Task ScrapeTvShowAsync(
            int showId,
            IAsyncPolicy<HttpResponseMessage> retryPolicy,
            CancellationToken cancellationToken
        )
        {
            var response = await retryPolicy.ExecuteAsync(
                () => _httpClient.GetAsync($"shows/{showId}?embed=cast", cancellationToken)
            );

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var show = JsonConvert.DeserializeObject<TvShow>(content);

                var result = JObject.Parse(content);
                var embeddedToken = result.SelectToken("_embedded.cast");
                var castMembers = new List<CastMember>();

                foreach (var item in embeddedToken)
                {
                    castMembers.Add(item.SelectToken("person")?.ToObject<CastMember>());
                }

                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                    await AddTvShow(dbContext, show, castMembers, cancellationToken);
                }
            }
        }

        private async Task AddTvShow(
            DataContext dbContext,
            TvShow tvShow,
            List<CastMember> castMembers,
            CancellationToken cancellationToken
        )
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var existingShow = await dbContext.TvShows.FirstOrDefaultAsync(s =>
                        s.Id == tvShow.Id
                    );

                    if (existingShow == null) // add new show
                    {
                        dbContext.TvShows.Add(tvShow);
                        await dbContext.SaveChangesAsync(cancellationToken);

                        var distinctMembers = castMembers
                            .GroupBy(c => c.Id)
                            .Select(g => g.First())
                            .ToList();

                        foreach (var member in distinctMembers)
                        {
                            var existingMember = await dbContext.CastMembers.FirstOrDefaultAsync(
                                m => m.Id == member.Id,
                                cancellationToken
                            );
                            if (existingMember == null)
                            {
                                // Add new cast member
                                dbContext.CastMembers.Add(member);
                                tvShow.CastMembers.Add(member);
                            }
                            else
                            {
                                // Add existing cast member to the show
                                tvShow.CastMembers.Add(existingMember);
                            }
                        }
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Database operation failed", ex);
                }
            }
        }
    }
}
