// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOBot
{
    static class NamesList
    {
        private static readonly string[] namesArray =
        {
            "AlphaAA",
            "BravoBB",
            "Charlie",
            "DeltaDD",
            "EchoEE",
            "Foxtrot",
            "GolfGG",
            "HotelHH",
            "IndiaII",
            "Juliett",
            "KiloKK",
            "LimaLL",
            "MikeMM",
            "November",
            "OscarOO",
            "PapaPO",
            "Quebec",
            "RomeoRR",
            "Sierra",
            "TangoTT",
            "Uniform",
            "Victor",
            "Whiskey",
            "XrayXX",
            "Yankee"
        };

        public static string Get(int index)
        {
            if (index < 0 || index >= namesArray.Length)
                index = 0;
            return namesArray[index];
        }

        public static string Rand()
        {
            Random gen = new Random();
            int len = gen.Next(5, 11);
            string ret = "";
            for (int i = 0; i < len; ++i)
            {
                ret += Convert.ToChar(Convert.ToInt32('a') + gen.Next(0, 25));
            }
            return ret;
        }
    }
}
