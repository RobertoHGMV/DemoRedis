using DemoRedis.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace DemoRedis.Api.Controllers
{
    [Route("api/v1")]
    public class ContriesController : Controller
    {
        private readonly IDistributedCache _distributedCache;
        private const string CountriesKey = "Contries";
        private const string ContinentsKey = "Continents";

        public ContriesController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        [HttpGet]
        [Route("continents")]
        public async Task<IActionResult> GetContinents()
        {
            try
            {
                var continentsObject = await _distributedCache.GetStringAsync(ContinentsKey);

                if (!string.IsNullOrWhiteSpace(continentsObject))
                    return Ok(continentsObject);
                else
                {
                    const string restContinentUrl = "http://www.geonames.org/childrenJSON?geonameId=6295630";

                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync(restContinentUrl);

                        var responseData = await response.Content.ReadAsStringAsync();

                        var viewModel = JsonConvert.DeserializeObject<ContinentViewModel>(responseData);

                        var memoryCacheEntryOptions = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                            SlidingExpiration = TimeSpan.FromSeconds(1200)
                        };

                        await _distributedCache.SetStringAsync(ContinentsKey, responseData, memoryCacheEntryOptions);

                        return Ok(viewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //[HttpGet]
        //[Route("countries")]
        //public async Task<IActionResult> GetCountries()
        //{
        //    try
        //    {
        //        var contriesObject = await _distributedCache.GetStringAsync(CountriesKey);

        //        if (!string.IsNullOrWhiteSpace(contriesObject))
        //            return Ok(contriesObject);
        //        else
        //        {
        //            var countries = new List<Country>();

        //            foreach (var continent in await GetAllContinents())
        //            {
        //                var restCountriesUrl = $"http://www.geonames.org/childrenJSON?geonameId={continent.GeonameId}";

        //                using (var httpClient = new HttpClient())
        //                {
        //                    var response = await httpClient.GetAsync(restCountriesUrl);

        //                    var responseData = await response.Content.ReadAsStringAsync();

        //                    var viewModel = JsonConvert.DeserializeObject<CountryViewModel>(responseData);

        //                    countries.AddRange(viewModel.Geonames);
        //                }
        //            }

        //            var bytesArray = new List<byte>();
        //            countries.ForEach(c => { bytesArray.Add(Convert.ToByte(c)); });

        //            var memoryCacheEntryOptions = new DistributedCacheEntryOptions
        //            {
        //                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
        //                SlidingExpiration = TimeSpan.FromSeconds(1200)
        //            };

        //            await _distributedCache.SetAsync(CountriesKey, bytesArray.ToArray(), memoryCacheEntryOptions);

        //            return Ok(countries);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex);
        //    }
        //}

        private async Task<List<Continent>> GetAllContinents()
        {
            var continentsObject = await _distributedCache.GetStringAsync(ContinentsKey);

            if (!string.IsNullOrWhiteSpace(continentsObject)) 
            {
                var viewModel = JsonConvert.DeserializeObject<ContinentViewModel>(continentsObject);
                return viewModel.Geonames;
            }
            else
            {
                const string restContinentUrl = "http://www.geonames.org/childrenJSON?geonameId=6295630";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(restContinentUrl);

                    var responseData = await response.Content.ReadAsStringAsync();

                    var viewModel = JsonConvert.DeserializeObject<ContinentViewModel>(responseData);

                    var memoryCacheEntryOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                        SlidingExpiration = TimeSpan.FromSeconds(1200)
                    };

                    await _distributedCache.SetStringAsync(ContinentsKey, responseData, memoryCacheEntryOptions);

                    return viewModel.Geonames;
                }
            }
        }
    }
}
