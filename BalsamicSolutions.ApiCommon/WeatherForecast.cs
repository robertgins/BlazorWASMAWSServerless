using System;
using System.Collections.Generic;
using System.Text;

namespace BalsamicSolutions.ApiCommon
{
    /// <summary>
    /// Just used for testing connectivity
    /// </summary>
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string LoggedInUser { get; set; }
    }
}
