using System;

namespace telegrammBot.Data
{
    public class Weather
    {
        public string City { get; set; }
        public int Temperature { get; set; }
        public string Photo { get; set; }
        public DateTime Date { get; set; }
    }
}