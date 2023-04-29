using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib.IO;
using EOLib.IO.Services;

namespace EOLib.Net
{
    public class PacketBuilder : IPacketBuilder
    {
        private const byte BREAK_STR_MAXVAL = 121;

        private readonly IReadOnlyList<byte> _data;
        private readonly INumberEncoderService _encoderService;

        public int Length => _data.Count;

        public PacketFamily Family => (PacketFamily)_data[1];

        public PacketAction Action => (PacketAction) _data[0];

        public PacketBuilder()
        {
            _data = new List<byte>();
            _encoderService = new NumberEncoderService();
        }

        public PacketBuilder(PacketFamily family, PacketAction action)
        {
            _data = new List<byte> { (byte) action, (byte) family };
            _encoderService = new NumberEncoderService();
        }

        private PacketBuilder(IReadOnlyList<byte> data)
        {
            _data = data;
            _encoderService = new NumberEncoderService();
        }

        public IPacketBuilder WithFamily(PacketFamily family)
        {
            var newData = new List<byte>(_data);
            newData[1] = (byte)family;
            return new PacketBuilder(newData);
        }

        public IPacketBuilder WithAction(PacketAction action)
        {
            var newData = new List<byte>(_data);
            newData[0] = (byte)action;
            return new PacketBuilder(newData);
        }

        public IPacketBuilder AddBreak()
        {
            return AddByte(byte.MaxValue);
        }

        public IPacketBuilder AddByte(byte b)
        {
            return AddBytes(new[] { b });
        }

        public IPacketBuilder AddChar(int b)
        {
            ThrowIfOutOfBounds(b, NumericConstants.ONE_BYTE_MAX);
            return AddBytes(_encoderService.EncodeNumber(b, 1));
        }

        public IPacketBuilder AddShort(int s)
        {
            ThrowIfOutOfBounds(s, NumericConstants.TWO_BYTE_MAX);
            return AddBytes(_encoderService.EncodeNumber(s, 2));
        }

        public IPacketBuilder AddThree(int t)
        {
            ThrowIfOutOfBounds(t, NumericConstants.THREE_BYTE_MAX);
            return AddBytes(_encoderService.EncodeNumber(t, 3));
        }

        public IPacketBuilder AddInt(int i)
        {
            ThrowIfOutOfBounds(i, NumericConstants.FOUR_BYTE_MAX);
            return AddBytes(_encoderService.EncodeNumber(i, 4));
        }

        public IPacketBuilder AddString(string s)
        {
            return AddBytes(Encoding.ASCII.GetBytes(s));
        }

        public IPacketBuilder AddBreakString(string s)
        {
            var sBytes = Encoding.ASCII.GetBytes(s);
            var sList = sBytes.Select(b => b == byte.MaxValue ? BREAK_STR_MAXVAL : b).ToList();
            sList.Add(byte.MaxValue);
            return AddBytes(sList);
        }

        public IPacketBuilder AddBytes(IEnumerable<byte> bytes)
        {
            var list = new List<byte>(_data);
            list.AddRange(bytes);
            return new PacketBuilder(list);
        }

        public IPacket Build()
        {
            return new Packet(_data.ToList());
        }

        private static void ThrowIfOutOfBounds(int input, uint maximum)
        {
            if ((uint)input > maximum)
            {
                throw new ArgumentException($"Input value is out of bounds (value={input}, bounds={maximum})", nameof(input));
            }
        }
    }
}
