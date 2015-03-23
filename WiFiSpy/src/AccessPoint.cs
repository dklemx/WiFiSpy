using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFiSpy.src.Packets;

namespace WiFiSpy.src
{
    public class AccessPoint
    {
        public BeaconFrame BeaconFrame { get; private set; }
        private List<BeaconFrame> BeaconFrames { get; set; }

        public string SSID
        {
            get
            {
                return BeaconFrame.SSID;
            }
        }

        public string MacAddress
        {
            get
            {
                return BeaconFrame.MacAddressStr;
            }
        }

        public string Manufacturer
        {
            get
            {
                return BeaconFrame.Manufacturer;
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return BeaconFrame.TimeStamp;
            }
        }

        public AccessPoint(BeaconFrame beaconFrame)
        {
            this.BeaconFrame = beaconFrame;
            this.BeaconFrames = new List<BeaconFrame>();
        }

        internal void AddBeaconFrame(BeaconFrame beaconFrame)
        {
            this.BeaconFrames.Add(beaconFrame);
        }



        public override string ToString()
        {
            return "[" + MacAddress + "] " + SSID;
        }
    }
}