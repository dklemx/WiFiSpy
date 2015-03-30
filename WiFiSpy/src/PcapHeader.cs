using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WiFiSpy.src
{
    /// <summary>
    ///  A wrapper for libpcap's pcap_pkthdr structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)] // Force it to match a 32-bit native header exactly
    public struct PcapHeader
    {
        static readonly bool isWindows = Environment.OSVersion.Platform != PlatformID.Unix;
        static readonly bool is32BitTs = IntPtr.Size == 4 || isWindows;

        /// <summary>
        ///  A wrapper class for libpcap's pcap_pkthdr structure
        /// </summary>
        private unsafe PcapHeader(IntPtr pcap_pkthdr)
        {
            if (!isWindows)
            {
                var pkthdr = *(PcapUnmanagedStructures.pcap_pkthdr_unix*)pcap_pkthdr;
                this.CaptureLength = pkthdr.caplen;
                this.PacketLength = pkthdr.len;
                this.Seconds = (uint)pkthdr.ts.tv_sec;
                this.MicroSeconds = (uint)pkthdr.ts.tv_usec;
            }
            else
            {
                var pkthdr = *(PcapUnmanagedStructures.pcap_pkthdr_windows*)pcap_pkthdr;
                this.CaptureLength = pkthdr.caplen;
                this.PacketLength = pkthdr.len;
                this.Seconds = (uint)pkthdr.ts.tv_sec;
                this.MicroSeconds = (uint)pkthdr.ts.tv_usec;
            }
        }

        /// <summary>
        /// Get a PcapHeader structure from a pcap_pkthdr pointer.
        /// </summary>
        public unsafe static PcapHeader FromPointer(IntPtr pcap_pkthdr)
        {
            if (is32BitTs)
            {
                return *(PcapHeader*)pcap_pkthdr;
            }
            else
            {
                return new PcapHeader(pcap_pkthdr);
            }
        }

        /// <summary>
        /// Constructs a new PcapHeader
        /// </summary>
        /// <param name="seconds">The seconds value of the packet's timestamp</param>
        /// <param name="microseconds">The microseconds value of the packet's timestamp</param>
        /// <param name="packetLength">The actual length of the packet</param>
        /// <param name="captureLength">The length of the capture</param>
        public PcapHeader(uint seconds, uint microseconds, uint packetLength, uint captureLength)
        {
            this.Seconds = seconds;
            this.MicroSeconds = microseconds;
            this.PacketLength = packetLength;
            this.CaptureLength = captureLength;
        }

        /// <summary>
        /// The seconds value of the packet's timestamp
        /// </summary>
        public uint Seconds;

        /// <summary>
        /// The microseconds value of the packet's timestamp
        /// </summary>
        public uint MicroSeconds;

        /// <summary>
        /// The length of the packet on the line
        /// </summary>
        public uint PacketLength;

        /// <summary>
        /// The the bytes actually captured. If the capture length
        /// is small CaptureLength might be less than PacketLength
        /// </summary>
        public uint CaptureLength;

        /// <summary>
        /// DateTime(1970, 1, 1).Ticks, saves cpu cycles in the Date property
        /// </summary>
        const long epochTicks = 621355968000000000L;

        /// <summary>
        /// Return the DateTime value of this pcap header
        /// </summary>
        public System.DateTime Date
        {
            get
            {
                return new DateTime(epochTicks + (Seconds * 10000000L) + (MicroSeconds * 10L));
            }
        }

        /// <summary>
        /// Marshal this structure into the platform dependent version and return
        /// and IntPtr to that memory
        ///
        /// NOTE: IntPtr MUST BE FREED via Marshal.FreeHGlobal()
        /// </summary>
        /// <returns>
        /// A <see cref="IntPtr"/>
        /// </returns>
        public IntPtr MarshalToIntPtr()
        {
            IntPtr hdrPtr;

            if (!isWindows)
            {
                // setup the structure to marshal
                var pkthdr = new PcapUnmanagedStructures.pcap_pkthdr_unix();
                pkthdr.caplen = this.CaptureLength;
                pkthdr.len = this.PacketLength;
                pkthdr.ts.tv_sec = (IntPtr)this.Seconds;
                pkthdr.ts.tv_usec = (IntPtr)this.MicroSeconds;

                hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PcapUnmanagedStructures.pcap_pkthdr_unix)));
                Marshal.StructureToPtr(pkthdr, hdrPtr, true);
            }
            else
            {
                var pkthdr = new PcapUnmanagedStructures.pcap_pkthdr_windows();
                pkthdr.caplen = this.CaptureLength;
                pkthdr.len = this.PacketLength;
                pkthdr.ts.tv_sec = (int)this.Seconds;
                pkthdr.ts.tv_usec = (int)this.MicroSeconds;

                hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PcapUnmanagedStructures.pcap_pkthdr_windows)));
                Marshal.StructureToPtr(pkthdr, hdrPtr, true);
            }

            return hdrPtr;
        }
    }
}
