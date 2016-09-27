// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.IO.Map;

namespace EOLib.IO.Services
{
    public class ChestSpawnMapEntitySerializer : IMapEntitySerializer<ChestSpawnMapEntity>
    {
        private readonly INumberEncoderService _numberEncoderService;

        public int DataSize
        {
            get { return 12; }
        }

        public MapEntitySerializeType MapEntitySerializeType
        {
            get { return MapEntitySerializeType.ChestEntitySerializer; }
        }

        public ChestSpawnMapEntitySerializer(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public byte[] SerializeToByteArray(ChestSpawnMapEntity mapEntity)
        {
            var retBytes = new List<byte>(DataSize);

            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.X, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Y, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber((short) mapEntity.Key, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Slot, 1));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.ItemID, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.RespawnTime, 2));
            retBytes.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Amount, 3));

            return retBytes.ToArray();
        }

        public ChestSpawnMapEntity DeserializeFromByteArray(byte[] data)
        {
            if (data.Length != DataSize)
                throw new ArgumentException("Data is improperly sized for deserialization", "data");

            return new ChestSpawnMapEntity
            {
                X = _numberEncoderService.DecodeNumber(data[0]),
                Y = _numberEncoderService.DecodeNumber(data[1]),
                Key = (ChestKey) _numberEncoderService.DecodeNumber(data[2], data[3]),
                Slot = (byte) _numberEncoderService.DecodeNumber(data[4]),
                ItemID = (short) _numberEncoderService.DecodeNumber(data[5], data[6]),
                RespawnTime = (short) _numberEncoderService.DecodeNumber(data[7], data[8]),
                Amount = _numberEncoderService.DecodeNumber(data[9], data[10], data[11])
            };
        }
    }
}
