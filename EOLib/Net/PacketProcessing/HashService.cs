// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net.PacketProcessing
{
    public class HashService : IHashService
    {
        public int StupidHash(int seed)
        {
            ++seed;
            return 110905 + (seed % 9 + 1) * ((11092004 - seed) % ((seed % 11 + 1) * 119)) * 119 + seed % 2004;
        }
    }

    public interface IHashService
    {
        int StupidHash(int seed);
    }
}
