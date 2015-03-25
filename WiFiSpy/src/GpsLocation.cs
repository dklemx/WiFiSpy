using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFiSpy.src
{
    public class GpsLocation
    {
        public DateTime Time { get; private set; }
        public double Longitude { get; private set; }
        public double Latitude { get; private set; }

        public GpsLocation(DateTime Time, double Longitude, double Latitude)
        {
            this.Time = Time;
            this.Longitude = Longitude;
            this.Latitude = Latitude;
        }

        public override string ToString()
        {
            return "Date:" + Time.ToString("dd-MM-yyyy HH:mm:ss") + ", long:" + Longitude + ", lat:" + Latitude;
        }
    }
}