using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiFiSpy.src.Packets;

namespace WiFiSpy.src
{
    public class CapFile
    {
        private List<BeaconFrame> _beacons;
        private SortedList<long, AccessPoint> _accessPoints;
        private SortedList<string, AccessPoint[]> _APExtenders;
        private SortedList<long, Station> _stations;
        private List<DataFrame> _dataFrames;

        public BeaconFrame[] Beacons
        {
            get
            {
                return _beacons.ToArray();
            }
        }

        public AccessPoint[] AccessPoints
        {
            get
            {
                return _accessPoints.Values.ToArray();
            }
        }
        public Station[] Stations
        {
            get
            {
                return _stations.Values.ToArray();
            }
        }
        public DataFrame[] DataFrames
        {
            get
            {
                return _dataFrames.ToArray();
            }
        }

        public SortedList<string, AccessPoint[]> PossibleExtenders
        {
            get
            {
                if (_APExtenders != null)
                    return _APExtenders;

                SortedList<string, List<AccessPoint>> extenders = new SortedList<string, List<AccessPoint>>();

                foreach (AccessPoint AP in _accessPoints.Values)
                {
                    if (!AP.BeaconFrame.IsHidden)
                    {
                        if (!extenders.ContainsKey(AP.SSID))
                                extenders.Add(AP.SSID, new List<AccessPoint>());

                        extenders[AP.SSID].Add(AP);
                    }
                }

                //only copy now the ones that are having more then 1 AP (Extender)
                SortedList<string, AccessPoint[]> temp = new SortedList<string, AccessPoint[]>();

                for (int i = 0; i < extenders.Count; i++)
                {
                    if (extenders.Values[i].Count > 1)
                    {
                        temp.Add(extenders.Keys[i], extenders.Values[i].ToArray());
                    }
                }

                this._APExtenders = temp;
                return _APExtenders;
            }
        }

        public CapFile(string FilePath)
        {
            /**/ICaptureDevice device = null;

            try
            {
                // Get an offline device
                device = new CaptureFileReaderDevice(FilePath);

                // Open the device
                device.Open();
            }
            catch (Exception e)
            {
                return;
            }

            _beacons = new List<BeaconFrame>();
            _accessPoints = new SortedList<long, AccessPoint>();
            _stations = new SortedList<long, Station>();
            _dataFrames = new List<DataFrame>();

            device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            device.Capture();
            device.Close();

            //CapFileReader reader = new CapFileReader();
            //reader.ReadCapFile(FilePath);

            //link all the DataFrames to the Stations
            foreach (Station station in _stations.Values)
            {
                long MacSourceAddrNumber = MacToLong(station.SourceMacAddress);

                for(int i = 0; i < _dataFrames.Count; i++)
                {
                    if(_dataFrames[i].MacSourceAddressLong == MacSourceAddrNumber || _dataFrames[i].MacTargetAddressLong == MacSourceAddrNumber)
                    {
                        station.AddDataFrame(_dataFrames[i]);
                    }
                }
            }
        }

        internal static long MacToLong(byte[] MacAddress)
        {
            byte[] MacAddrTemp = new byte[8];
            Array.Copy(MacAddress, MacAddrTemp, 6);
            return BitConverter.ToInt64(MacAddrTemp, 0);
        }

        int packetsProcessed = 0;
        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            packetsProcessed++;

            if (packetsProcessed == 38)
            {

            }

            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ieee80211)
            {
                Packet packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                PacketDotNet.Ieee80211.BeaconFrame beacon = packet as PacketDotNet.Ieee80211.BeaconFrame;
                PacketDotNet.Ieee80211.ProbeRequestFrame probeRequest = packet as PacketDotNet.Ieee80211.ProbeRequestFrame;
                PacketDotNet.Ieee80211.QosDataFrame DataFrame = packet as PacketDotNet.Ieee80211.QosDataFrame;

                PacketDotNet.Ieee80211.DeauthenticationFrame DeAuthFrame = packet as PacketDotNet.Ieee80211.DeauthenticationFrame;
                PacketDotNet.Ieee80211.AssociationRequestFrame AuthFrame2 = packet as PacketDotNet.Ieee80211.AssociationRequestFrame;


                PacketDotNet.Ieee80211.DataDataFrame DataDataFrame = packet as PacketDotNet.Ieee80211.DataDataFrame;


                if (beacon != null)
                {
                    BeaconFrame beaconFrame = new BeaconFrame(beacon, e.Packet.Timeval.Date);
                    _beacons.Add(beaconFrame);

                    long MacAddrNumber = MacToLong(beaconFrame.MacAddress);

                    //check for APs with this Mac Address
                    AccessPoint AP = null;

                    if (!_accessPoints.TryGetValue(MacAddrNumber, out AP))
                    {
                        AP = new AccessPoint(beaconFrame);
                        _accessPoints.Add(MacAddrNumber, AP);
                    }
                    AP.AddBeaconFrame(beaconFrame);
                }
                else if (probeRequest != null)
                {
                    ProbePacket probe = new ProbePacket(probeRequest, e.Packet.Timeval.Date);
                    Station station = null;

                    long MacAddrNumber = MacToLong(probe.SourceMacAddress);

                    if (!_stations.TryGetValue(MacAddrNumber, out station))
                    {
                        station = new Station(probe);
                        _stations.Add(MacAddrNumber, station);
                    }

                    station.AddProbe(probe);
                }
                else if (DataFrame != null)
                {
                    DataFrame _dataFrame = new Packets.DataFrame(DataFrame, e.Packet.Timeval.Date);

                    //invalid packets are useless, probably encrypted
                    if (_dataFrame.IsValidPacket)
                    {
                        _dataFrames.Add(_dataFrame);
                    }
                }
                else if (DataDataFrame != null)
                {

                }
            }
        }
    }
}