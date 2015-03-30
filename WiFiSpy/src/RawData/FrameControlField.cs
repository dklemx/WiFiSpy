using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFiSpy.src.RawData
{
    public class FrameControlField
    {
        public byte ProtocolVersion { get; private set; }
        public bool ToDS { get; private set; }
        public bool FromDS { get; private set; }
        public bool MoreFragment { get; private set; }
        public bool ReTry { get; private set; }
        public bool PowerManagement { get; private set; }
        public bool MoreData { get; private set; }
        public bool IsWep { get; private set; }
        public bool Order { get; private set; }

        public FrameTypes Type { get; private set; }
        public FrameSubTypes SubType { get; private set; }

        public FrameControlField(ushort value)
        {
            this.ProtocolVersion = (byte)((value >> 0x8) & 0x3);
            this.ToDS = ((value & 0x1) == 1) ? true : false;
            this.FromDS = (((value >> 1) & 0x1) == 1) ? true : false;
            this.MoreFragment = (((value >> 2) & 0x1) == 1) ? true : false;
            this.ReTry = (((value >> 3) & 0x1) == 1) ? true : false;
            this.PowerManagement = (((value >> 4) & 0x1) == 1) ? true : false;
            this.MoreData = (((value >> 5) & 0x1) == 1) ? true : false;
            this.IsWep = (((value >> 6) & 0x1) == 1) ? true : false;
            this.Order = (((value >> 0x7) & 0x1) == 1) ? true : false;

            this.Type = (FrameTypes)(((value >> 8) & 0xC) >> 2);
            this.SubType = (FrameSubTypes)((((value >> 8) & 0x0C) << 2) | ((value >> 8) >> 4));
        }
    }
}