using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColourBlanketWeb.Models;
using DarkSky.Models;

namespace ColourBlanketWeb
{
    public interface IBlanketService
    {
        Task<IEnumerable<BlanketDay>> GetDays(double latitude, double longitude, DateTime startDate,
            DateTime endDate);
    }
    
    public class BlanketService : IBlanketService
    {
        private const string DarkSkyApiKey = "API_KEY_HERE";
        public async Task<IEnumerable<BlanketDay>> GetDays(double latitude, double longitude, DateTime startDate, DateTime endDate)
        {
            var darkSky = new DarkSky.Services.DarkSkyService(DarkSkyApiKey);

            var forecastTasks = new Dictionary<DateTime, Task<DarkSkyResponse>>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var forecastTask = darkSky.GetForecast(latitude, longitude,
                    new OptionalParameters {MeasurementUnits = "uk2", ForecastDateTime = date});
                forecastTasks.Add(date, forecastTask);
            }

            return await MapForecasts(forecastTasks);
        }

        private static async Task<IEnumerable<BlanketDay>> MapForecasts(Dictionary<DateTime, Task<DarkSkyResponse>> forecastTasks)
        {
            var forecasts = await Task.WhenAll(forecastTasks.Select(async pair =>
                new KeyValuePair<DateTime, DarkSkyResponse>(pair.Key, await pair.Value)));

            return forecasts.Select(x => MapForecast(x)).OrderByDescending(x => x.Date);
        }

        private static BlanketDay MapForecast(in KeyValuePair<DateTime, DarkSkyResponse> kvp)
        {
            var response = kvp.Value.Response;
            var day = new BlanketDay
            {
                Date = kvp.Key,
                Minimum = MapTemperature(response.Daily.Data[0].TemperatureMin.Value),
                Maximum = MapTemperature(response.Daily.Data[0].TemperatureMax.Value),
                Average = MapTemperature(response.Hourly.Data.Select(x => x.Temperature.Value).Average())
            };
            return day;
        }

        private static BlanketTemperature MapTemperature(double temperature)
        {
            var temperatureRounded = Convert.ToInt32(Math.Round(temperature, 0, MidpointRounding.AwayFromZero));

            return new BlanketTemperature
            {
                ColourName = DeriveColourName(temperatureRounded),
                Temperature = temperatureRounded
            };
        }

        private static string DeriveColourName(int temperature)
        {
            if (temperature <= -4) return "Midnight";

            if (temperature == -3 || temperature == -2) return "Lobelia";

            if (temperature == -1 || temperature == 0) return "Cloud Blue";

            if (temperature == 1 || temperature == 2) return "Parma Violet";

            if (temperature == 3 || temperature == 4) return "Empire";

            if (temperature == 5 || temperature == 6) return "Turquoise";

            return "todo";

            // TODO
        }
    }
}