// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
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
                throw new ArgumentException("Data is improperly sized for serialization", "mapEntity");

            return mapEntity.RawData;
        }

        public UnknownMapEntity DeserializeFromByteArray(byte[] data)
        {
            if (data.Length != UnknownMapEntity.DATA_SIZE)
                throw new ArgumentException("Data is improperly sized for deserialization", "data");

            return new UnknownMapEntity
            {
                X = _numberEncoderService.DecodeNumber(data[0]),
                Y = _numberEncoderService.DecodeNumber(data[1]),
                RawData = data
            };
        }
    }
}
