using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Logger;

namespace EOLib.Net.PacketProcessing
{
    [AutoMappedType]
    public class PacketProcessActions : IPacketProcessActions
    {
        private readonly IPacketEncoderService _encoderService;
        private readonly IPacketSequenceService _sequenceService;
        private readonly ILoggerProvider _loggerProvider;
        private readonly IPacketEncoderRepository _encoderRepository;
        private readonly ISequenceRepository _sequenceRepository;

        public PacketProcessActions(ISequenceRepository sequenceNumberRepository,
                                    IPacketEncoderRepository encoderRepository,
                                    IPacketEncoderService encoderService,
                                    IPacketSequenceService sequenceService,
                                    ILoggerProvider loggerProvider)
        {
            _sequenceRepository = sequenceNumberRepository;
            _encoderRepository = encoderRepository;

            _encoderService = encoderService;
            _sequenceService = sequenceService;
            _loggerProvider = loggerProvider;
        }

        public void SetInitialSequenceNumber(int seq1, int seq2)
        {
            var initialSequence = _sequenceService.CalculateInitialSequenceNumber(seq1, seq2);
            _sequenceRepository.SequenceStart = initialSequence;
        }

        public void SetUpdatedBaseSequenceNumber(int seq1, int seq2)
        {
            var updatedSequence = _sequenceService.CalculateNewInitialSequenceNumber(seq1, seq2);
            _sequenceRepository.SequenceStart = updatedSequence;
        }

        public void SetEncodeMultiples(int emulti_d, int emulti_e)
        {
            _encoderRepository.ReceiveMultiplier = emulti_d;
            _encoderRepository.SendMultiplier = emulti_e;

            _loggerProvider.Logger.Log("**** PACKET ENCODING MULTIPLES FOR THIS SESSION ARE: RECV={0} SEND={1}",
                                       _encoderRepository.ReceiveMultiplier, _encoderRepository.SendMultiplier);
        }

        public byte[] EncodePacket(IPacket pkt)
        {
            var seq = CalculateNextSequenceNumber();
            pkt = _encoderService.AddSequenceNumber(pkt, seq);

            var data = _encoderService.Encode(pkt, _encoderRepository.SendMultiplier);
            data = _encoderService.PrependLengthBytes(data);

            return data;
        }

        public byte[] EncodeRawPacket(IPacket pkt)
        {
            return _encoderService.PrependLengthBytes(pkt.RawData.ToArray());
        }

        public IPacket DecodeData(IEnumerable<byte> rawData)
        {
            return _encoderService.Decode(rawData, _encoderRepository.ReceiveMultiplier);
        }

        private int CalculateNextSequenceNumber()
        {
            var oldSequenceIncrement = _sequenceRepository.SequenceIncrement;
            var sequenceStart = _sequenceRepository.SequenceStart;

            _sequenceRepository.SequenceIncrement = _sequenceService.CalculateNextSequenceIncrement(oldSequenceIncrement);
            return _sequenceService.CalculateNextSequenceNumber(sequenceStart, _sequenceRepository.SequenceIncrement);
        }
    }

    public interface IPacketProcessActions
    {
        void SetInitialSequenceNumber(int seq1, int seq2);

        void SetUpdatedBaseSequenceNumber(int seq1, int seq2);

        void SetEncodeMultiples(int emulti_d, int emulti_e);

        byte[] EncodePacket(IPacket pkt);

        byte[] EncodeRawPacket(IPacket pkt);

        IPacket DecodeData(IEnumerable<byte> rawData);
    }
}
