using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFiSpy.src.Packets
{
    public class DataFrame
    {
        public PacketDotNet.Ieee80211.QosDataFrame Frame { get; private set; }
        private int PayloadOffset;
        private int PayloadLen;
        public DateTime TimeStamp { get; private set; }

        public byte[] SourceMacAddress
        {
            get
            {
                return Frame.SourceAddress.GetAddressBytes();
            }
        }

        public string SourceMacAddressStr
        {
            get
            {
                return BitConverter.ToString(SourceMacAddress);
            }
        }


        public byte[] TargetMacAddress
        {
            get
            {
                return Frame.DestinationAddress.GetAddressBytes();
            }
        }

        public string TargetMacAddressStr
        {
            get
            {
                return BitConverter.ToString(TargetMacAddress);
            }
        }

        public long MacSourceAddressLong { get; private set; }
        public long MacTargetAddressLong { get; private set; }

        public bool isIPv4 { get; private set; }
        public bool isTCP { get; private set; }
        public bool isUDP { get; private set; }

        public string SourceIp { get; private set; }
        public string DestIp { get; private set; }

        public int PortSource { get; private set; }
        public int PortDest { get; private set; }

        public bool IsValidPacket
        {
            get
            {
                return isIPv4 && (isTCP || isUDP);
            }
        }

        public byte[] Payload
        {
            get
            {
                if (PayloadOffset > 0 && PayloadLen > 0)
                {
                    byte[] ret = new byte[PayloadLen];
                    Array.Copy(Frame.Bytes, PayloadOffset, ret, 0, ret.Length);
                    return ret;
                }
                return new byte[0];
            }
        }

        public DataFrame(PacketDotNet.Ieee80211.QosDataFrame DataFrame, DateTime TimeStamp)
        {
            this.Frame = DataFrame;
            this.TimeStamp = TimeStamp;

            this.MacSourceAddressLong = CapFile.MacToLong(SourceMacAddress);
            this.MacTargetAddressLong = CapFile.MacToLong(TargetMacAddress);

            //a hacky "parser" to get to the actual payload to get what we need
            int ReadOffset = 0;
            ReadOffset += 26; //IEEE 802.11 header
            ReadOffset += 8; //Logical-Link Control header

            byte[] PacketData = DataFrame.Bytes;

            if(!(ReadOffset + 20 /*IPv4*/ + 5 > PacketData.Length)) //5byte Packet size as reserved... who wants to read a ~10byte packet anyway
            {
                this.isIPv4 = PacketData[ReadOffset] == 0x45;
                this.isTCP = PacketData[ReadOffset + 9] == 0x06;
                this.isUDP = PacketData[ReadOffset + 9] == 0x11;

                if (isIPv4 && (isTCP || isUDP))
                {
                    SourceIp = PacketData[ReadOffset + 12] + "." + PacketData[ReadOffset + 13] + "." + PacketData[ReadOffset + 14] + "." + PacketData[ReadOffset + 15];
                    DestIp = PacketData[ReadOffset + 16] + "." + PacketData[ReadOffset + 17] + "." + PacketData[ReadOffset + 18] + "." + PacketData[ReadOffset + 19];

                    ReadOffset += 20; //IPv4 header

                    if (isTCP)
                    {
                        PortSource = PacketData[ReadOffset] << 8 | PacketData[ReadOffset + 1];
                        PortDest = PacketData[ReadOffset + 2] << 8 | PacketData[ReadOffset + 3];

                        ReadOffset += 32; //TCP header
                    }

                    if (isUDP)
                    {
                        PortSource = PacketData[ReadOffset] << 8 | PacketData[ReadOffset + 1];
                        PortDest = PacketData[ReadOffset + 2] << 8 | PacketData[ReadOffset + 3];

                        ReadOffset += 8; //UDP header
                    }

                    this.PayloadLen = PacketData.Length - ReadOffset;
                    this.PayloadOffset = ReadOffset;
                }
            }

            if (String.IsNullOrEmpty(SourceIp) || String.IsNullOrEmpty(DestIp))
            {
                SourceIp = "";
                DestIp = "";
            }
        }

        public string GetHttpLocation()
        {
            string PayloadStr = ASCIIEncoding.ASCII.GetString(Payload);

            if (PayloadStr.ToLower().Contains("host:") || PayloadStr.ToLower().Contains("location:") )
            {
                string[] temp = PayloadStr.Split('\n');

                if (temp != null && temp.Length > 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].ToLower().StartsWith("host:"))
                        {
                            return temp[i].Substring(5);
                        }
                        if (temp[i].ToLower().StartsWith("location:"))
                        {
                            return temp[i].Substring(9);
                        }
                    }
                }
            }
            return "";
        }

        public override string ToString()
        {
            if (!isIPv4 && !isTCP && !isUDP)
                return "";

            string TempPayloadStr = ASCIIEncoding.ASCII.GetString(Payload, 0, Payload.Length > 512 ? 512 : Payload.Length);
            return "[" + SourceIp + "] -> [" + DestIp + "] " + TempPayloadStr;
        }
    }
}