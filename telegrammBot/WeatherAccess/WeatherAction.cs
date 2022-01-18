using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using telegrammBot.Data;
using telegrammBot.WeatherAccess.ApiData;

namespace telegrammBot.WeatherAccess
{
    public static class WeatherAction
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<Weather> GetCurrentWeather(string city)
        { 
            var response = JsonConvert.DeserializeObject<CurrentDateResponse>(await (await httpClient.GetAsync(
                            $"http://api.weatherapi.com/v1/current.json?key={BotData.WeatherKey}&q={city}&aqi=yes")).Content.ReadAsStringAsync());

            if (response == null)
                return null;

            return new Weather
            {
                City = response.Location.name, 
                Date = response.Location.localtime,
                Photo = response.Current.condition.text == "Sunny" ? "https://cdn.icon-icons.com/icons2/8/PNG/256/sunrise_sun_sunny_shower_showers_sunny_cloudy_fog_day_time_1458.png" : "", 
                Temperature = (int) response.Current.temp_c
            };
        }

        public static async Task<Weather> GetCurrentWeather(double lat, double lon)
        {
            var response = JsonConvert.DeserializeObject<CurrentDateResponse>(await (await httpClient.GetAsync(
                $"http://api.weatherapi.com/v1/current.json?key={BotData.WeatherKey}&q={lat},{lon}&aqi=yes")).Content.ReadAsStringAsync());

            if (response == null)
                return null;

            return new Weather
            {
                City = response.Location.name,
                Date = response.Location.localtime,
                Photo = response.Current.condition.text == "Sunny" ? "https://cdn.icon-icons.com/icons2/8/PNG/256/sunrise_sun_sunny_shower_showers_sunny_cloudy_fog_day_time_1458.png" : "",
                Temperature = (int)response.Current.temp_c
            };
        }
    }
}
