using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    [MappedType(BaseType = typeof(IMapEntitySerializer<NPCSpawnMapEntity>))]
    [MappedType(BaseType = typeof(IMapDeserializer<NPCSpawnMapEntity>))]
    public class NPCSpawnMapEntitySerializer : IMapEntitySerializer<NPCSpawnMapEntity>
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
                throw new ArgumentException("Data is improperly sized for deserialization", nameof(data));

            return new NPCSpawnMapEntity()
                .WithX(_numberEncoderService.DecodeNumber(data[0]))
                .WithY(_numberEncoderService.DecodeNumber(data[1]))
                .WithID(_numberEncoderService.DecodeNumber(data[2], data[3]))
                .WithSpawnType(_numberEncoderService.DecodeNumber(data[4]))
                .WithRespawnTime(_numberEncoderService.DecodeNumber(data[5], data[6]))
                .WithAmount(_numberEncoderService.DecodeNumber(data[7]));
        }
    }
}
