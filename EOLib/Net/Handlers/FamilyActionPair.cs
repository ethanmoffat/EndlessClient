using System;
using System.Collections;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Handlers
{
    public readonly struct FamilyActionPair : IEqualityComparer
    {
        public PacketFamily Family { get; }
        public PacketAction Action { get; }

        public FamilyActionPair(PacketFamily family, PacketAction action)
        {
            Family = family;
            Action = action;
        }

        bool IEqualityComparer.Equals(object obj1, object obj2)
        {
            if (!(obj1 is FamilyActionPair) || !(obj2 is FamilyActionPair))
                return false;

            var fap1 = (FamilyActionPair)obj1;
            var fap2 = (FamilyActionPair)obj2;
            return fap1.Family == fap2.Family && fap1.Action == fap2.Action;
        }

        public int GetHashCode(object obj)
        {
            if (!(obj is FamilyActionPair)) return 0;

            var fap /*lol*/ = (FamilyActionPair)obj;

            return (int)fap.Family << 8 & (byte)fap.Action;
        }

        public static FamilyActionPair From(byte[] array)
        {
            if (array.Length < 2)
                throw new ArgumentException("Unable to determine packet ID from input array");

            return new FamilyActionPair((PacketFamily)array[1], (PacketAction)array[0]);
        }
    }
}
