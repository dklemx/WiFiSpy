using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFiSpy.src.Packets;

namespace WiFiSpy.src
{
    public class AccessPoint
    {
        public BeaconFreame BeaconFrame { get; private set; }
        private List<BeaconFreame> BeaconFrames { get; set; }

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

        public AccessPoint(BeaconFreame beaconFrame)
        {
            this.BeaconFrame = beaconFrame;
            this.BeaconFrames = new List<BeaconFreame>();
        }

        internal void AddBeaconFrame(BeaconFreame beaconFrame)
        {
            this.BeaconFrames.Add(beaconFrame);
        }



        public override string ToString()
        {
            return "[" + MacAddress + "] " + SSID;
        }
    }
}