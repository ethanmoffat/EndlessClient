// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    public class WarpMapEntitySerializer : IMapEntitySerializer<WarpMapEntity>
    {
        private readonly INumberEncoderService numberEncoderService;

        public int DataSize { get { return 8; } }

        public MapEntitySerializeType MapEntitySerializeType
        {
            get { return MapEntitySerializeType.WarpEntitySerializer; }
        }

        public WarpMapEntitySerializer(INumberEncoderService numberEncoderService)
        {
            this.numberEncoderService = numberEncoderService;
        }

        public byte[] SerializeToByteArray(WarpMapEntity mapEntity)
        {
            var retBytes = new List<byte>(DataSize);

            retBytes.AddRange(numberEncoderService.EncodeNumber(mapEntity.X, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(mapEntity.DestinationMapID, 2));
            retBytes.AddRange(numberEncoderService.EncodeNumber(mapEntity.DestinationMapX, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(mapEntity.DestinationMapY, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(mapEntity.LevelRequirement, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber((short)mapEntity.DoorType, 2));

            return retBytes.ToArray();
        }

        public WarpMapEntity DeserializeFromByteArray(byte[] data)
        {
            if (data.Length != DataSize)
                throw new ArgumentException("Data is improperly sized for deserialization", "data");

            return new WarpMapEntity
            {
                X = numberEncoderService.DecodeNumber(data[0]),
                DestinationMapID = (short) numberEncoderService.DecodeNumber(data[1], data[2]),
                DestinationMapX = (byte) numberEncoderService.DecodeNumber(data[3]),
                DestinationMapY = (byte) numberEncoderService.DecodeNumber(data[4]),
                LevelRequirement = (byte) numberEncoderService.DecodeNumber(data[5]),
                DoorType = (DoorSpec) numberEncoderService.DecodeNumber(data[6], data[7])
            };
        }
    }
}
