// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EOLib.Localization
{
    public class EDFFile : IEDFFile
    {
        public Dictionary<int, string> Data { get; private set; }

        public EDFFile(string fileName, DataFiles whichFile)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("File does not exist!", fileName);

            Data = new Dictionary<int, string>();

            if (whichFile == DataFiles.CurseFilter)
            {
                string[] lines = File.ReadAllLines(fileName);
                int i = 0;
                foreach (string encoded in lines)
                {
                    string decoded = _decodeDatString(encoded, whichFile);
                    string[] curses = decoded.Split(':');
                    foreach (string curse in curses)
                        Data.Add(i++, curse);
                }
            }
            else
            {
                string[] lines = File.ReadAllLines(fileName, Encoding.Default);
                int i = 0;
                foreach (string encoded in lines)
                    Data.Add(i++, _decodeDatString(encoded, whichFile));
            }
        }

        private string _decodeDatString(string input, DataFiles whichFile)
        {
            //unencrypted
            if (whichFile == DataFiles.Credits || whichFile == DataFiles.Checksum)
                return input;

            string ret = "";

            for (int i = 0; i < input.Length; i += 2)
                ret += input[i];

            //if there are an even number of characters start with the last one
            //otherwise start with the second to last one
            int startIndex = input.Length - (input.Length % 2 == 0 ? 1 : 2);
            for (int i = startIndex; i >= 0; i -= 2)
                ret += input[i];

            if (whichFile == DataFiles.CurseFilter)
                return ret;

            StringBuilder sb = new StringBuilder(ret);

            //adjacent ascii char values that are multiples of 7 should be flipped
            for (int i = 0; i < sb.Length; ++i)
            {
                int next = i + 1;
                if (next < sb.Length)
                {
                    char c1 = sb[i], c2 = sb[next];
                    int ch1 = Convert.ToInt32(c1);
                    int ch2 = Convert.ToInt32(c2);

                    if (ch1 % 7 == 0 && ch2 % 7 == 0)
                    {
                        sb[i] = c2;
                        sb[next] = c1;
                    }
                }
            }

            return sb.ToString();
        }
    }

    public interface IEDFFile
    {
        Dictionary<int, string> Data { get; }
    }
}
