using System;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    [MappedType(BaseType = typeof(IMapEntitySerializer<WarpMapEntity>))]
    [MappedType(BaseType = typeof(IMapDeserializer<WarpMapEntity>))]
    public class WarpMapEntitySerializer : IMapEntitySerializer<WarpMapEntity>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public WarpMapEntitySerializer(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public byte[] SerializeToByteArray(WarpMapEntity mapEntity)
        {
            var retBytes = new List<byte>(WarpMapEntity.DATA_SIZE);

            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.X, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.DestinationMapID, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.DestinationMapX, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.DestinationMapY, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.LevelRequirement, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber((int)mapEntity.DoorType, 2));

            return retBytes.ToArray();
        }

        public WarpMapEntity DeserializeFromByteArray(byte[] data)
        {
            if (data.Length != WarpMapEntity.DATA_SIZE)
                throw new ArgumentException("Data is improperly sized for deserialization", nameof(data));

            return new WarpMapEntity()
                .WithX(_numberEncoderService.DecodeNumber(data[0]))
                .WithDestinationMapID(_numberEncoderService.DecodeNumber(data[1], data[2]))
                .WithDestinationMapX(_numberEncoderService.DecodeNumber(data[3]))
                .WithDestinationMapY(_numberEncoderService.DecodeNumber(data[4]))
                .WithLevelRequirement(_numberEncoderService.DecodeNumber(data[5]))
                .WithDoorType((DoorSpec) _numberEncoderService.DecodeNumber(data[6], data[7]));
        }
    }
}
