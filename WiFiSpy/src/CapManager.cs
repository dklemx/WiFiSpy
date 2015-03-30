using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFiSpy.src.Packets;

namespace WiFiSpy.src
{
    public class CapManager
    {
        /// <summary>
        /// Get all stations from the cap files without duplicates
        /// </summary>
        /// <param name="CapFiles"></param>
        /// <returns></returns>
        public static Station[] GetStations(CapFile[] CapFiles)
        {
            SortedList<string, Station> StationMacs = new SortedList<string, Station>();

            foreach (CapFile capFile in CapFiles)
            {
                foreach (Station station in capFile.Stations)
                {
                    if (!String.IsNullOrEmpty(station.SourceMacAddressStr))
                    {
                        if (!StationMacs.ContainsKey(station.SourceMacAddressStr))
                        {
                            StationMacs.Add(station.SourceMacAddressStr, station);
                        }
                        else
                        {
                            Station _station = StationMacs[station.SourceMacAddressStr];

                            //merge the data from this point...

                            //copy the probes from other cap files
                            foreach (ProbePacket probe in station.Probes)
                            {
                                if (_station.DataFrames.FirstOrDefault(o => o.TimeStamp.Ticks == probe.TimeStamp.Ticks) == null)
                                {
                                    _station.AddProbe(probe);
                                }
                            }

                            //copy the data frames from other cap files
                            foreach(DataFrame frame in station.DataFrames)
                            {
                                if(_station.DataFrames.FirstOrDefault(o => o.TimeStamp.Ticks == frame.TimeStamp.Ticks) == null)
                                {
                                    _station.AddDataFrame(frame);
                                }
                            }
                        }
                    }
                }
            }

            return StationMacs.Values.ToArray();
        }
    }
}