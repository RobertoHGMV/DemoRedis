using System;
using System.Collections.Generic;

namespace DemoRedis.Api.Models
{
    public class CountryViewModel
    {
        public int TotalResultsCount { get; set; }
        public List<Country> Geonames { get; set; }
    }
}
