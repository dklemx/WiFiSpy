using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using WiFiSpy.src.RawData;

namespace WiFiSpy.src
{
    public class CapFileReader
    {
        internal const int MAX_PACKET_SIZE = 65536;
        private const int PCAP_ERRBUF_SIZE = 256;

        [DllImport("wpcap", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr /* pcap_t* */ pcap_open_offline(string/*const char* */ fname, StringBuilder/* char* */ errbuf);

        /// <summary>
        /// The delegate declaration for PcapHandler requires an UnmanagedFunctionPointer attribute.
        /// Without this it fires for one time and then throws null pointer exception
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void pcap_handler(IntPtr param, IntPtr /* pcap_pkthdr* */ header, IntPtr pkt_data);

        /// <summary>
        /// Read packets until cnt packets are processed or an error occurs.
        /// </summary>
        [DllImport("wpcap", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private extern static int pcap_dispatch(IntPtr /* pcap_t* */ adaptHandle, int count, pcap_handler callback, IntPtr ptr);

        /// <summary> Return the link layer of an adapter. </summary>
        [DllImport("wpcap", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private extern static int pcap_datalink(IntPtr /* pcap_t* */ adaptHandle);

        private IntPtr PcapHandle = IntPtr.Zero;

        public CapFileReader()
        {

        }

        public void ReadCapFile(string FilePath)
        {
            StringBuilder errbuf = new StringBuilder(PCAP_ERRBUF_SIZE);
            PcapHandle = pcap_open_offline(FilePath, errbuf);

            // handle error
            if (PcapHandle == IntPtr.Zero)
            {
                throw new Exception("Unable to open offline adapter: " + errbuf.ToString());
            }

            pcap_handler Callback = new pcap_handler(PacketHandler);

            int ret = pcap_dispatch(PcapHandle, -1 /* -1 = infinite */, Callback, IntPtr.Zero);


        }

        /// <summary>
        /// Pcap_loop callback method.
        /// </summary>
        private void PacketHandler(IntPtr param, IntPtr /* pcap_pkthdr* */ header, IntPtr data)
        {
            MarshalRawPacket(header, data);

            //var p = MarshalRawPacket(header, data);
            //SendPacketArrivalEvent(p);
        }

        private void MarshalRawPacket(IntPtr /* pcap_pkthdr* */ header, IntPtr data)
        {
            //RawCapture p;

            // marshal the header
            PcapHeader pcapHeader = PcapHeader.FromPointer(header);

            var PacketData = new byte[pcapHeader.CaptureLength];
            Marshal.Copy(data, PacketData, 0, (int)pcapHeader.CaptureLength);

            LinkLayers linkType = (LinkLayers)pcap_datalink(PcapHandle);

            //we're only interested in the WiFi Traffic
            if (linkType == LinkLayers.Ieee80211 && PacketData.Length > 2)
            {
                ushort FrameFieldValue = (ushort)(PacketData[0] << 8 | PacketData[1]);
                FrameControlField field = new FrameControlField(FrameFieldValue);

                switch (field.SubType)
                {
                    case FrameSubTypes.ManagementBeacon:
                    {

                        break;
                    }
                }
            }

            //p = new RawCapture(LinkType, new PosixTimeval(pcapHeader.Seconds, pcapHeader.MicroSeconds), pkt_data);

        }
    }
}