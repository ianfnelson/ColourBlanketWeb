using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ColourBlanketWeb.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ColourBlanketWeb.Controllers
{
    public class HomeController : Controller
    {
        private const double WistowLatitude = 53.812174;
        private const double WistowLongitude = -1.096165;
        private readonly IMemoryCache _cache;
        private readonly IBlanketService _blanketService;

        public HomeController(IMemoryCache cache, IBlanketService blanketService)
        {
            _cache = cache;
            _blanketService = blanketService;
        }
        
        public IActionResult Index()
        {
            ViewData["ColourBlanket"] = _cache.GetOrCreate("temperatures", entry =>
                {
                    entry.SetSlidingExpiration(new TimeSpan(0, 6, 0, 0));

                    return _blanketService.GetDays(WistowLatitude, WistowLongitude, DateTime.Today.AddDays(-28),
                        DateTime.Today).Result;
                });
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}