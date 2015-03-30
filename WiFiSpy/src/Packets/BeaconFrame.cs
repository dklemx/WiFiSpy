using PacketDotNet;
using PacketDotNet.Ieee80211;
using PacketDotNet.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFiSpy.src.Packets
{
    public class BeaconFrame
    {
        public string Manufacturer { get; private set; }
        public string SSID { get; private set; }
        public bool IsHidden { get; private set; }
        public byte[] MacAddress { get; private set; }
        public int Channel { get; private set; }
        public DateTime TimeStamp { get; private set; }

        public string MacAddressStr
        {
            get
            {
                if (MacAddress != null)
                    return BitConverter.ToString(MacAddress);
                return "";
            }
        }

        private PacketDotNet.Ieee80211.BeaconFrame Frame;

        public BeaconFrame(PacketDotNet.Ieee80211.BeaconFrame frame, DateTime TimeStamp)
        {
            this.Frame = frame;
            this.Manufacturer = OuiParser.GetOuiByMac(frame.SourceAddress.GetAddressBytes());
            this.MacAddress = frame.SourceAddress.GetAddressBytes();
            this.TimeStamp = TimeStamp;

            foreach (InformationElement element in frame.InformationElements)
            {
                switch (element.Id)
                {
                    case InformationElement.ElementId.ServiceSetIdentity:
                    {
                        SSID = ASCIIEncoding.ASCII.GetString(element.Value);

                        if (SSID != null && SSID.Length > 0)
                        {
                            IsHidden = SSID[0] == 0;
                        }
                        break;
                    }
                    case InformationElement.ElementId.DsParameterSet:
                    {
                        if (element.Value != null && element.Value.Length >= 3)
                        {
                            Channel = element.Value[2];
                        }
                        break;
                    }
                }
            }

            if (String.IsNullOrEmpty(SSID))
                SSID = "";
        }
    }
}