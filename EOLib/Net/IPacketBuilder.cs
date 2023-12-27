using System.Collections.Generic;

namespace EOLib.Net
{
    public interface IPacketBuilder
    {
        int Length { get; }

        PacketFamily Family { get; }

        PacketAction Action { get; }

        IPacketBuilder WithFamily(PacketFamily family);

        IPacketBuilder WithAction(PacketAction action);

        IPacketBuilder AddBreak();

        IPacketBuilder AddByte(byte b);

        IPacketBuilder AddChar(int b);

        IPacketBuilder AddShort(int s);

        IPacketBuilder AddThree(int t);

        IPacketBuilder AddInt(int i);

        IPacketBuilder AddString(string s);

        IPacketBuilder AddBreakString(string s);

        IPacketBuilder AddBytes(IEnumerable<byte> bytes);

        IPacket Build();
    }
}
