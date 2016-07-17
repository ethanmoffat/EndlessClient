// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Net
{
    public enum PacketAction : byte
    {
        Request = (byte)1,
        Accept = (byte)2,
        Reply = (byte)3,
        Remove = (byte)4,
        Agree = (byte)5,
        Create = (byte)6,
        Add = (byte)7,
        Player = (byte)8,
        Take = (byte)9,
        Use = (byte)10,
        Buy = (byte)11,
        Sell = (byte)12,
        Open = (byte)13,
        Close = (byte)14,
        Message = (byte)15,
        Spec = (byte)16,
        Admin = (byte)17,
        List = (byte)18,
        Tell = (byte)20,
        Report = (byte)21,
        Announce = (byte)22,
        Server = (byte)23,
        Drop = (byte)24,
        Junk = (byte)25,
        Obtain = (byte)26,
        Get = (byte)27,
        Kick = (byte)28,
        Rank = (byte)29,
        TargetSelf = (byte)30,
        TargetOther = (byte)31,
        TargetGroup = (byte)33,
        Exp = (byte)33, //PACKET_TARGET_GROUP
        Dialog = (byte)34,
        Ping = (byte)240,
        Pong = (byte)241,
        Net3 = (byte)242,
        Init = (byte)255
    }
}
