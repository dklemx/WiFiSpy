using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WiFiSpy.src
{
    public class OuiParser
    {
        private static SortedList<int, string> OuiNames;

        public static void Initialize(string FilePath)
        {
            OuiNames = new SortedList<int, string>();

            if (!File.Exists(FilePath))
                return;

            using (StreamReader sr = new StreamReader(FilePath))
            {
                string line = null;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("(hex)"))
                    {
                        try
                        {
                            byte FirstByte = Convert.ToByte(line.Substring(2, 2), 16);
                            byte SecondByte = Convert.ToByte(line.Substring(5, 2), 16);
                            byte ThirdByte = Convert.ToByte(line.Substring(8, 2), 16);
                            string Name = line.Substring(20);

                            int IntAddr = FirstByte | SecondByte << 8 | ThirdByte << 16;

                            if (!OuiNames.ContainsKey(IntAddr))
                            {
                                OuiNames.Add(IntAddr, Name);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        public static string GetOuiByMac(byte[] MacAddress)
        {
            lock (OuiNames)
            {
                if (MacAddress != null && MacAddress.Length >= 3)
                {
                    int IntAddr = MacAddress[0] | MacAddress[1] << 8 | MacAddress[2] << 16;
                    string ret = "";
                    if (!OuiNames.TryGetValue(IntAddr, out ret))
                        return "";
                    return ret;
                }
                return "";
            }
        }
    }
}