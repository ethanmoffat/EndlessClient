// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;

namespace EOLib.Net.PacketProcessing
{
    public class PacketProcessActions : IPacketProcessorActions
    {
        private readonly IPacketEncoderService _encoderService;
        private readonly IPacketSequenceService _sequenceService;
        private readonly IPacketEncoderRepository _encoderRepository;
        private readonly ISequenceRepository _sequenceRepository;

        public PacketProcessActions(ISequenceRepository sequenceNumberRepository,
                                    IPacketEncoderRepository encoderRepository,
                                    IPacketEncoderService encoderService,
                                    IPacketSequenceService sequenceService)
        {
            _sequenceRepository = sequenceNumberRepository;
            _encoderRepository = encoderRepository;

            _encoderService = encoderService;
            _sequenceService = sequenceService;
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

        public void SetEncodeMultiples(byte emulti_d, byte emulti_e)
        {
            _encoderRepository.ReceiveMultiplier = emulti_d;
            _encoderRepository.SendMultiplier = emulti_e;
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
}
