// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    public class NPCSpawnMapEntitySerializer : ISerializer<NPCSpawnMapEntity>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public NPCSpawnMapEntitySerializer(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public byte[] SerializeToByteArray(NPCSpawnMapEntity mapEntity)
        {
            var retBytes = new List<byte>(NPCSpawnMapEntity.DATA_SIZE);

            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.X, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Y, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.ID, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.SpawnType, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.RespawnTime, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Amount, 1));

            return retBytes.ToArray();
        }

        public NPCSpawnMapEntity DeserializeFromByteArray(byte[] data)
        {
            if (data.Length != NPCSpawnMapEntity.DATA_SIZE)
                throw new ArgumentException("Data is improperly sized for deserialization", "data");

            return new NPCSpawnMapEntity()
                .WithX(_numberEncoderService.DecodeNumber(data[0]))
                .WithY(_numberEncoderService.DecodeNumber(data[1]))
                .WithID((short) _numberEncoderService.DecodeNumber(data[2], data[3]))
                .WithSpawnType((byte) _numberEncoderService.DecodeNumber(data[4]))
                .WithRespawnTime((short) _numberEncoderService.DecodeNumber(data[5], data[6]))
                .WithAmount((byte) _numberEncoderService.DecodeNumber(data[7]));
        }
    }
}
