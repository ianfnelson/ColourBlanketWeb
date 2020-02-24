using System;

namespace ColourBlanketWeb.Models
{
    public class BlanketDay
    {
        public DateTime Date { get; set; }
        
        public BlanketTemperature Minimum { get; set; }
        
        public BlanketTemperature Maximum { get; set; }
        
        public BlanketTemperature Average { get; set; }
    }
}