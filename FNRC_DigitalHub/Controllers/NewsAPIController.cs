using FNRC_DigitalHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Xml.Linq;

namespace FNRC_DigitalHub.Controllers
{
    [ApiController]
    [Route("NewsAPI")]
    [AllowAnonymous]
    public class NewsAPIController(IConfiguration configuration, IMemoryCache cache) : BaseController
    {
        private readonly IConfiguration configuration = configuration;
        private readonly IMemoryCache cache = cache;
        private readonly string _cacheKey = "CombinedNewsData";



        public async Task<IActionResult> GetExternalNews(string fromDate, string toDate)
        {
            string cacheKey = $"News_{fromDate}_{toDate}";

            // 1. Try to get data from Cache
            if (!cache.TryGetValue(cacheKey, out string cachedXml))
            {
                using var client = new HttpClient();
                string newsBaseURL = configuration["APIURLS:NewsURL"];
                string website_url = $"{newsBaseURL}?fromdate={fromDate}&todate={toDate}&keyword=&IsSearched=1";
                string manara_news = configuration["APIURLS:ManaraNews"];
                try
                {
                    var response = await client.GetAsync(website_url);
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


        [HttpGet("GetExternalNews")]
        public async Task<List<UnifiedNewsViewModel>> GetNewsAsync()
        {
            if (!cache.TryGetValue(_cacheKey, out List<UnifiedNewsViewModel> combinedList))
            {
                combinedList = await FetchAndCombineNews();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                    .SetSlidingExpiration(TimeSpan.FromHours(1));

                cache.Set(_cacheKey, combinedList, cacheEntryOptions);
            }

            return combinedList;
        }

        private async Task<List<UnifiedNewsViewModel>> FetchAndCombineNews()
        {
            var list = new List<UnifiedNewsViewModel>();
            var now = DateTime.UtcNow;
            var fromDate = new DateTime(now.Year, now.Month, 1).AddMonths(-2).ToString("yyyy-MM-dd");
            var toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            using var client = new HttpClient();
            string newsBaseURL = configuration["APIURLS:NewsURL"];
            string website_url = $"{newsBaseURL}?fromdate={fromDate}&todate={toDate}&keyword=&IsSearched=1";
            string manara_news = configuration["APIURLS:ManaraNews"];

            try
            {
                var response = await client.GetAsync(website_url);
                if (response.IsSuccessStatusCode)
                {
                    string res = await response.Content.ReadAsStringAsync();
                    var jsonData = JsonConvert.DeserializeObject<dynamic>(res);
                    
                        foreach (var item in jsonData)
                        {
                            list.Add(new UnifiedNewsViewModel
                            {
                                Id = item.ID.ToString(),
                                TitleEn = item.TitleEnglish,
                                TitleAr = item.TitleArabic,
                                DescriptionEn = item.ShortEnglish,
                                DescriptionAr = item.ShortArabic, 
                                PublishDate = item.DateCreated,
                                Source = "website"
                            });
                        }
                    
                } 
  
            }
            catch (Exception) { }

            try
            {
                var jsonResponse = await client.GetStringAsync(manara_news);
                var jsonData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                if (jsonData?.success == true)
                {
                    foreach (var item in jsonData.data)
                    {
                        list.Add(new UnifiedNewsViewModel
                        {
                            Id = item.Id.ToString(),
                            TitleEn = item.NameEn,
                            TitleAr = item.NameAr,
                            DescriptionEn = item.DescriptionEn,
                            DescriptionAr = item.DescriptionAr,
                            ImageUrl = item.NewsEventAttachments?.Count > 0 ? item.NewsEventAttachments[0].Path : "",
                            PublishDate = item.NewsEventDate,
                            Source = "manara"
                        });
                    }
                }
            }
            catch (Exception) { }

            return list.OrderByDescending(x => x.PublishDate).ToList();
        }
    }


}
