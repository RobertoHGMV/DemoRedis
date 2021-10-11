using DemoRedis.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DemoRedis.Api.Controllers
{
    [Route("api/v1")]
    public class EmployeeController : Controller
    {
        private readonly IDistributedCache _distributedCache;

        public EmployeeController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        [HttpGet]
        [Route("employees")]
        public async Task<IActionResult> Index()
        {
            var cacheKey = "Employees";
            var employees = new List<Employee>();

            var json = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrWhiteSpace(json))
            {
                employees = JsonSerializer.Deserialize<List<Employee>>(json);
            }
            else
            {
                var memoryCacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                    SlidingExpiration = TimeSpan.FromSeconds(1200)
                };

                employees = GetEmployees();
                json = JsonSerializer.Serialize<List<Employee>>(employees);
                await _distributedCache.SetStringAsync(cacheKey, json, memoryCacheEntryOptions);
            }

            return Ok(employees);
        }

        private List<Employee> GetEmployees()
        {
            return new List<Employee> 
            {
                new Employee("1", "Goku", 34),
                new Employee("2", "Gohan", 18),
                new Employee("3", "Vegeta", 36)
            };
        }
    }
}
