using System;
using AutomaticTypeMapper;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    [MappedType(BaseType = typeof(IMapEntitySerializer<UnknownMapEntity>))]
    [MappedType(BaseType = typeof(IMapDeserializer<UnknownMapEntity>))]
    public class UnknownMapEntitySerializer : IMapEntitySerializer<UnknownMapEntity>
    {
        private readonly INumberEncoderService _numberEncoderService;

        // todo: sausage figured out this is legacy door key data (x/y/key id (short)), rename accordingly
        public UnknownMapEntitySerializer(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public byte[] SerializeToByteArray(UnknownMapEntity mapEntity)
        {
            if (mapEntity.RawData.Length != UnknownMapEntity.DATA_SIZE)
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
