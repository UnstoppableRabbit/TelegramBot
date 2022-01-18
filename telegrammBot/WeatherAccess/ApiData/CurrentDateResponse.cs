using System;

namespace telegrammBot.WeatherAccess.ApiData
{
    public class CurrentDateResponse
    {
        public location Location { get; set; }
        public current Current { get; set; }
    }

    [Serializable]
    public class location
    {
        public string name { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string tz_id { get; set; }
        public DateTime localtime { get; set; }
    }

    [Serializable]
    public class current
    {
        public double temp_c { get; set; }
        public double temp_f { get; set; }
        public bool is_day { get; set; }
        public string wind_kph { get; set; }
        public int humidity { get; set; }
        public condition condition { get; set; }
    }

    [Serializable]
    public class condition
    {
        public string text { get; set; }
        public string icon { get; set; }
        public int code { get; set; }
    }
}
