using System;
using AutomaticTypeMapper;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    [MappedType(BaseType = typeof(ISerializer<UnknownMapEntity>))]
    public class UnknownMapEntitySerializer : ISerializer<UnknownMapEntity>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public UnknownMapEntitySerializer(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public byte[] SerializeToByteArray(UnknownMapEntity mapEntity)
        {
            if(mapEntity.RawData.Length != UnknownMapEntity.DATA_SIZE)
                throw new ArgumentException("Data is improperly sized for serialization", nameof(mapEntity));

            return mapEntity.RawData;
        }

        public UnknownMapEntity DeserializeFromByteArray(byte[] data)
        {
            if (data.Length != UnknownMapEntity.DATA_SIZE)
                throw new ArgumentException("Data is improperly sized for deserialization", nameof(data));

            return new UnknownMapEntity()
                .WithX(_numberEncoderService.DecodeNumber(data[0]))
                .WithY(_numberEncoderService.DecodeNumber(data[1]))
                .WithRawData(data);
        }
    }
}
