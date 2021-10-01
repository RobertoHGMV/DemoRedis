using DemoRedis.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoRedis.Api.Controllers
{
    [Route("api/[controller]")]
    public class ContriesController : Controller
    {
        private readonly IDistributedCache _distributedCache;
        private const string CountriesKey = "Contries";

        public ContriesController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetContries()
        {
            var contriesObject = await _distributedCache.GetStringAsync(CountriesKey);

            if (!string.IsNullOrWhiteSpace(contriesObject))
                return Ok(contriesObject);
            else
            {
                const string restCountriesUrl = "https://restcountries.eu/rest/v2/all";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(restCountriesUrl);
                    
                    var responseData = await response.Content.ReadAsStringAsync();
                    
                    var contries = JsonConvert.DeserializeObject<List<Country>>(responseData);
                    
                    var memoryCacheEntryOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                        SlidingExpiration = TimeSpan.FromSeconds(1200)
                    };

                    await _distributedCache.SetStringAsync(CountriesKey, responseData, memoryCacheEntryOptions);

                    return Ok(contries);
                }
            }
        }
    }
}
