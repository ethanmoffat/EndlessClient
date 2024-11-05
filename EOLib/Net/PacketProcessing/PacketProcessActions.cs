using System;
using System.Net.Sockets;
using AutomaticTypeMapper;
using EOLib.Logger;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Optional;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType]
    public class PacketProcessActions : IPacketProcessActions
    {
        private readonly IPacketEncoderService _encoderService;
        private readonly IPacketEncoderRepository _encoderRepository;
        private readonly ISequenceRepository _sequenceRepository;

        public PacketProcessActions(ISequenceRepository sequenceNumberRepository,
                                    IPacketEncoderRepository encoderRepository,
                                    IPacketEncoderService encoderService)
        {
            _sequenceRepository = sequenceNumberRepository;
            _encoderRepository = encoderRepository;

            _encoderService = encoderService;
        }

        public void SetSequenceStart(ISequenceStart sequenceStart)
        {
            _sequenceRepository.Sequencer = _sequenceRepository.Sequencer.WithSequenceStart(sequenceStart);
            if (sequenceStart is InitSequenceStart)
            {
                _sequenceRepository.Sequencer.NextSequence();
            }
        }

        public void SetEncodeMultiples(int emulti_d, int emulti_e)
        {
            _encoderRepository.ReceiveMultiplier = emulti_d;
            _encoderRepository.SendMultiplier = emulti_e;
        }

        public byte[] EncodePacket(IPacket pkt)
        {
            var seq = _sequenceRepository.Sequencer.NextSequence();
            var data = _encoderService.Encode(pkt, _encoderRepository.SendMultiplier, seq);
            return PrependLengthBytes(data);
        }

        public byte[] EncodeRawPacket(IPacket pkt)
        {
            var eoWriter = new EoWriter();
            eoWriter.AddByte((byte)pkt.Action);
            eoWriter.AddByte((byte)pkt.Family);
            pkt.Serialize(eoWriter);
            return PrependLengthBytes(eoWriter.ToByteArray());
        }

        public Option<IPacket> DecodeData(byte[] rawData)
        {
            return _encoderService.Decode(rawData, _encoderRepository.ReceiveMultiplier);
        }

        private static byte[] PrependLengthBytes(byte[] data)
        {
            var newArray = new byte[data.Length + 2];
            Array.Copy(NumberEncoder.EncodeNumber(data.Length), newArray, 2);
            Array.Copy(data, 0, newArray, 2, data.Length);
            return newArray;
        }
    }

    public interface IPacketProcessActions
    {
        void SetSequenceStart(ISequenceStart sequenceStart);

        void SetEncodeMultiples(int emulti_d, int emulti_e);

        byte[] EncodePacket(IPacket pkt);

        byte[] EncodeRawPacket(IPacket pkt);

        Option<IPacket> DecodeData(byte[] rawData);
    }
}
