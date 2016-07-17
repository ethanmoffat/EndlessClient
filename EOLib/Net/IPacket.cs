// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.IO;

namespace EOLib.Net
{
    public interface IPacket
    {
        int Length { get; }

        int ReadPosition { get; }

        void Seek(int position, SeekOrigin origin);

        PacketFamily Family { get; }

        PacketAction Action { get; }

        IReadOnlyList<byte> RawData { get; }

        byte PeekByte();

        byte PeekChar();

        short PeekShort();

        int PeekThree();

        int PeekInt();

        string PeekString(int length);

        string PeekEndString();

        string PeekBreakString();

        IEnumerable<byte> PeekBytes(int length);

        byte ReadByte();

        byte ReadChar();

        short ReadShort();

        int ReadThree();

        int ReadInt();

        string ReadString(int length);

        string ReadEndString();

        string ReadBreakString();

        IEnumerable<byte> ReadBytes(int length);
    }
}
