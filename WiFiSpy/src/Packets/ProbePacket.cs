using PacketDotNet.Ieee80211;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFiSpy.src.Packets
{
    public class ProbePacket
    {
        public PacketDotNet.Ieee80211.ProbeRequestFrame ProbeRequestFrame { get; private set; }
        public byte[] SourceMacAddress
        {
            get
            {
                return ProbeRequestFrame.SourceAddress.GetAddressBytes();
            }
        }

        public string SourceMacAddressStr
        {
            get
            {
                return BitConverter.ToString(SourceMacAddress);
            }
        }

        public bool IsBroadCastProbe
        {
            get
            {
                for (int i = 0; i < SourceMacAddress.Length; i++)
                {
                    if (SourceMacAddress[i] != 0xFF)
                        return false;
                }
                return true;
            }
        }

        public string VendorSpecificManufacturer { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public string SSID { get; private set; }
        public bool IsHidden { get; private set; }

        public ProbePacket(PacketDotNet.Ieee80211.ProbeRequestFrame probeRequestFrame, DateTime TimeStamp)
        {
            this.ProbeRequestFrame = probeRequestFrame;
            this.TimeStamp = TimeStamp;

            foreach (InformationElement element in ProbeRequestFrame.InformationElements)
            {
                switch (element.Id)
                {
                    case InformationElement.ElementId.VendorSpecific:
                    {
                        VendorSpecificManufacturer = OuiParser.GetOuiByMac(element.Value);
                        break;
                    }
                    case InformationElement.ElementId.ServiceSetIdentity:
                    {
                        SSID = ASCIIEncoding.ASCII.GetString(element.Value);

                        if (SSID != null && SSID.Length > 0)
                        {
                            IsHidden = SSID[0] == 0;
                        }
                        else if(SSID == "")
                        {
                            IsHidden = true;
                        }
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return "[Probe] SSID: " + SSID + ", Is Hidden: " + IsHidden;
        }
    }
}
