// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections;

namespace EOLib.Net.Handlers
{
    public struct FamilyActionPair : IEqualityComparer
    {
        private readonly PacketFamily fam;
        private readonly PacketAction act;

        public FamilyActionPair(PacketFamily family, PacketAction action)
        {
            fam = family;
            act = action;
        }

        bool IEqualityComparer.Equals(object obj1, object obj2)
        {
            if (!(obj1 is FamilyActionPair) || !(obj2 is FamilyActionPair))
                return false;

            var fap1 = (FamilyActionPair) obj1;
            var fap2 = (FamilyActionPair)obj2;
            return fap1.fam == fap2.fam && fap1.act == fap2.act;
        }

        public int GetHashCode(object obj)
        {
            if (!(obj is FamilyActionPair)) return 0;

            var fap /*lol*/ = (FamilyActionPair)obj;

            return (int)fap.fam << 8 & (byte)fap.act;
        }
    }
}