using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WiFiSpy.src.GpsParsers
{
    public class GpsCsvParser
    {
        //I only used 1 csv file sample from a app, let's hope they're all using the same template
        public static GpsLocation[] GetLocations(string FilePath)
        {
            List<GpsLocation> locations = new List<GpsLocation>();

            using(StreamReader reader = new StreamReader(FilePath))
            {
                string line = "";
                
                while((line = reader.ReadLine()) != null)
                {
                    if (line.Length < 25 || !line.Contains(','))
                        continue;

                    string[] Values = line.Split(',');

                    string Date = Values[0];
                    string Lat = Values[1];
                    string Long = Values[2];

                    int year = 0;
                    int month = 0;
                    int day = 0;
                    int hour = 0;
                    int minute = 0;
                    int second = 0;

                    double Longitude = 0D;
                    double Latitude = 0D;

                    if(int.TryParse(Date.Substring(0, 4), out year) &&
                       int.TryParse(Date.Substring(5, 2), out month) &&
                       int.TryParse(Date.Substring(8, 2), out day) &&
                       int.TryParse(Date.Substring(11, 2), out hour) &&
                       int.TryParse(Date.Substring(14, 2), out minute) &&
                       int.TryParse(Date.Substring(17, 2), out second) &&
                       double.TryParse(Lat, out Latitude) &&
                       double.TryParse(Long, out Longitude))
                    {
                        locations.Add(new GpsLocation(new DateTime(year, month, day, hour, minute, second), Longitude, Latitude));
                    }
                }
            }
            return locations.ToArray();
        }
    }
}