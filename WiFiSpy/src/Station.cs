using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFiSpy.src.Packets;

namespace WiFiSpy.src
{
    public class Station
    {
        public ProbePacket InitialProbe { get; private set; }

        private List<ProbePacket> _probes;
        private List<DataFrame> _payloadTraffic;

        public ProbePacket[] Probes
        {
            get
            {
                return _probes.ToArray();
            }
        }
        public DataFrame[] DataFrames
        {
            get
            {
                return _payloadTraffic.OrderBy(o => o.TimeStamp.Ticks).ToArray();
            }
        }

        public byte[] SourceMacAddress
        {
            get
            {
                return InitialProbe.SourceMacAddress;
            }
        }

        public string SourceMacAddressStr
        {
            get
            {
                return BitConverter.ToString(SourceMacAddress);
            }
        }

        /// <summary>
        /// The name of the Station that is captured from the DHCP traffic
        /// </summary>
        public string StationName { get; internal set; }

        public DateTime TimeStamp
        {
            get
            {
                return InitialProbe.TimeStamp;
            }
        }

        public bool IsAndroidDevice
        {
            get
            {
                return GetDeviceUserAgent("android") != "";
            }
        }

        public bool IsIPhoneDevice
        {
            get
            {
                return GetDeviceUserAgent("iphone") != "";
            }
        }

        public bool IsIPadDevice
        {
            get
            {
                return GetDeviceUserAgent("ipad") != "";
            }
        }

        public bool IsMacDevice
        {
            get
            {
                return GetDeviceUserAgent("mac") != "";
            }
        }

        public string DeviceTypeStr
        {
            get
            {
                if (IsAndroidDevice)
                    return "Android";

                if (IsIPhoneDevice)
                    return "iPhone";

                if (IsIPadDevice)
                    return "iPad";

                if (IsMacDevice)
                    return "Mac OSX";

                //If it's unknown, let's look at the mac address rather then relying on HTTP Traffic

                if (Manufacturer.ToLower().Contains("apple"))
                    return "Apple";

                if (Manufacturer.ToLower().Contains("samsung") ||
                    Manufacturer.ToLower().Contains("xiaomi") ||
                    Manufacturer.ToLower().Contains("motorola"))
                    return "Android";

                if (Manufacturer.ToLower().Contains("nintendo"))
                    return "Nintendo Console";

                if (Manufacturer.ToLower().Contains("nvidia"))
                    return "NVidia Shield";


                return "";
            }
        }

        public string Manufacturer {get; private set; }

        public Station(ProbePacket InitialProbe)
        {
            this.InitialProbe = InitialProbe;
            this._probes = new List<ProbePacket>();
            this._payloadTraffic = new List<DataFrame>();
            this.Manufacturer = OuiParser.GetOuiByMac(SourceMacAddress);
        }

        internal void AddProbe(ProbePacket probe)
        {
            lock (_probes)
            {
                this._probes.Add(probe);
            }
        }

        internal void AddDataFrame(DataFrame dataFrame)
        {
            lock (_payloadTraffic)
            {
                this._payloadTraffic.Add(dataFrame);
            }
        }

        private string GetDeviceUserAgent(string contains)
        {
            foreach (DataFrame frame in DataFrames)
            {
                if (frame.IsValidPacket && frame.PortDest == 80 || frame.PortSource == 80)
                {
                    string PayloadStr = ASCIIEncoding.ASCII.GetString(frame.Payload);

                    if (PayloadStr.ToLower().Contains("user-agent:"))
                    {
                        string[] temp = PayloadStr.Split('\n');

                        if (temp != null && temp.Length > 0)
                        {
                            for (int i = 0; i < temp.Length; i++)
                            {
                                if (temp[i].ToLower().StartsWith("user-agent:"))
                                {
                                    if (temp[i].ToLower().Contains(contains))
                                    {
                                        return temp[i];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }
    }
}