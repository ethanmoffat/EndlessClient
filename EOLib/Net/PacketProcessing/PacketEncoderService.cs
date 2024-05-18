using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType]
    public sealed class PacketEncoderService : IPacketEncoderService
    {
        private const string PACKET_NAMESPACE = "EOLib.Protocol.Net.Server";

        private readonly IPacketFactory _packetFactory;

        public PacketEncoderService(IPacketFactoryFactory packetFactoryFactory) => _packetFactory = packetFactoryFactory.Create(PACKET_NAMESPACE);

        public byte[] Encode(IPacket packet, int encodeMultiplier, int sequenceNumber)
        {
            var writer = new EoWriter();
            writer.AddByte((byte)packet.Action);
            writer.AddByte((byte)packet.Family);
            AddSequenceBytes(writer, sequenceNumber);

            packet.Serialize(writer);

            if (encodeMultiplier == 0 || !PacketValidForEncode(packet))
                return writer.ToByteArray();

            var encodedBytes = DataEncrypter.SwapMultiples(writer.ToByteArray(), encodeMultiplier);
            encodedBytes = DataEncrypter.Interleave(encodedBytes);
            encodedBytes = DataEncrypter.FlipMSB(encodedBytes);
            return encodedBytes;
        }

        public IPacket Decode(byte[] original, int decodeMultiplier)
        {
            var decodedBytes = original;

            if (decodeMultiplier > 0 && !PacketValidForDecode(original))
            {
                decodedBytes = DataEncrypter.FlipMSB(decodedBytes);
                decodedBytes = DataEncrypter.Deinterleave(decodedBytes);
                decodedBytes = DataEncrypter.SwapMultiples(decodedBytes, decodeMultiplier);
            }

            return _packetFactory.Create(decodedBytes);
        }

        private static bool PacketValidForEncode(IPacket pkt)
        {
            return !IsInitPacket((byte)pkt.Family, (byte)pkt.Action);
        }

        private static bool PacketValidForDecode(byte[] data)
        {
            return data.Length >= 2 && !IsInitPacket(data[0], data[1]);
        }

        private static bool IsInitPacket(byte family, byte action)
        {
            return (PacketFamily)family == PacketFamily.Init &&
                   (PacketAction)action == PacketAction.Init;
        }

        private void AddSequenceBytes(EoWriter writer, int seq)
        {
            if (seq >= EoNumericLimits.CHAR_MAX)
                writer.AddShort(seq);
            else
                writer.AddChar(seq);
        }
    }

    public interface IPacketEncoderService
    {
        byte[] Encode(IPacket original, int encodeMultiplier, int sequenceNumber);

        IPacket Decode(byte[] original, int decodeMultiplier);
    }
}
