using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ColourBlanketWeb.Models;
using DarkSky.Models;
using Microsoft.Extensions.Caching.Memory;

namespace ColourBlanketWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemoryCache _cache;

        public HomeController(IMemoryCache cache)
        {
            _cache = cache;
        }
        
        public IActionResult Index()
        {
            ViewData["ColourBlanket"] = _cache.GetOrCreate("temperatures", entry =>
                {
                    entry.SetSlidingExpiration(new TimeSpan(0, 6, 0, 0));
                    return GetTemperatures();
                });
            
            return View();
        }

        private static List<string> GetTemperatures()
        {
            var darkSky = new DarkSky.Services.DarkSkyService("API_KEY_HERE");

            var date = DateTime.Today.AddDays(-28);

            var temperatures = new List<string>();
            
            do
            {
                var forecast = darkSky.GetForecast(53.812174, -1.096165,
                    new OptionalParameters{MeasurementUnits ="uk2", ForecastDateTime = date}).Result;

                var temperatureLow = Math.Round(forecast.Response.Daily.Data[0].TemperatureMin.Value, 0, MidpointRounding.AwayFromZero);
                var temperatureHigh = Math.Round(forecast.Response.Daily.Data[0].TemperatureMax.Value, 0,
                    MidpointRounding.AwayFromZero);
                var temperatureAverage =
                    Math.Round(forecast.Response.Hourly.Data.Select(x => x.Temperature.Value).Average(), 0, MidpointRounding.AwayFromZero);
                
                temperatures.Add($"Date: {date.ToShortDateString()};   Min: {temperatureLow,2};   Max: {temperatureHigh,2};   Avg: {temperatureAverage,2}");
                
                date = date.AddDays(1);
            } while (date<DateTime.Today);

            return temperatures;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}