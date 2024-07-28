using AutomaticTypeMapper;
using EOLib.IO.Map;
using System;
using System.Collections.Generic;

namespace EOLib.IO.Services.Serializers
{
    [MappedType(BaseType = typeof(IMapEntitySerializer<ChestSpawnMapEntity>))]
    [MappedType(BaseType = typeof(IMapDeserializer<ChestSpawnMapEntity>))]
    public class ChestSpawnMapEntitySerializer : IMapEntitySerializer<ChestSpawnMapEntity>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public ChestSpawnMapEntitySerializer(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public byte[] SerializeToByteArray(ChestSpawnMapEntity mapEntity)
        {
            var retBytes = new List<byte>(ChestSpawnMapEntity.DATA_SIZE);

            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.X, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Y, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber((int)mapEntity.Key, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Slot, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.ItemID, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.RespawnTime, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Amount, 3));

            return retBytes.ToArray();
        }

        public ChestSpawnMapEntity DeserializeFromByteArray(byte[] data)
        {
            if (data.Length != ChestSpawnMapEntity.DATA_SIZE)
                throw new ArgumentException("Data is improperly sized for deserialization", nameof(data));

            return new ChestSpawnMapEntity()
                .WithX(_numberEncoderService.DecodeNumber(data[0]))
                .WithY(_numberEncoderService.DecodeNumber(data[1]))
                .WithKey((ChestKey)_numberEncoderService.DecodeNumber(data[2], data[3]))
                .WithSlot(_numberEncoderService.DecodeNumber(data[4]))
                .WithItemID(_numberEncoderService.DecodeNumber(data[5], data[6]))
                .WithRespawnTime(_numberEncoderService.DecodeNumber(data[7], data[8]))
                .WithAmount(_numberEncoderService.DecodeNumber(data[9], data[10], data[11]));
        }
    }
}