using System.Collections.Generic;

namespace DemoRedis.Api.Models
{
    public class ContinentViewModel
    {
        public int TotalResultsCount { get; set; }
        public List<Continent> Geonames { get; set; }
    }
}
