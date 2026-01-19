using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace FNRC_DigitalHub.Controllers
{
    [ApiController]
    [Route("NewsAPI")]
    [AllowAnonymous]
    public class NewsAPIController(IConfiguration configuration, IMemoryCache cache) : BaseController
    {
        private readonly IConfiguration configuration = configuration;
        private readonly IMemoryCache cache = cache;

        [HttpGet("GetExternalNews")]
        public async Task<IActionResult> GetExternalNews(string fromDate, string toDate)
        {
            string cacheKey = $"News_{fromDate}_{toDate}";

            // 1. Try to get data from Cache
            if (!cache.TryGetValue(cacheKey, out string cachedXml))
            {
                using var client = new HttpClient();
                string newsBaseURL = configuration["APIURLS:NewsURL"];
                string url = $"{newsBaseURL}?fromdate={fromDate}&todate={toDate}&keyword=&IsSearched=1";
                try
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        cachedXml = await response.Content.ReadAsStringAsync();
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                        cache.Set(cacheKey, cachedXml, cacheOptions);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return Content(cachedXml, "application/xml");
        }



    }
}
