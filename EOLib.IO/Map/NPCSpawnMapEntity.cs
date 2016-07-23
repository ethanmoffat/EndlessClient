// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public class NPCSpawnMapEntity : IMapEntity
    {
        public int DataSize { get { return 8; } }

        public int X { get; set; }

        public int Y { get; set; }

        public short ID { get; set; }

        public byte SpawnType { get; set; }

        public short RespawnTime { get; set; }

        public byte Amount { get; set; }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                           IMapStringEncoderService mapStringEncoderService)
        {
            var retBytes = new List<byte>(DataSize);

            retBytes.AddRange(numberEncoderService.EncodeNumber(X, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(Y, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(ID, 2));
            retBytes.AddRange(numberEncoderService.EncodeNumber(SpawnType, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(RespawnTime, 2));
            retBytes.AddRange(numberEncoderService.EncodeNumber(Amount, 1));

            return retBytes.ToArray();
        }

        public void DeserializeFromByteArray(byte[] data,
                                             INumberEncoderService numberEncoderService,
                                             IMapStringEncoderService mapStringEncoderService)
        {
            if (data.Length != DataSize)
                throw new ArgumentException("Data is improperly sized for deserialization", "data");

            X = numberEncoderService.DecodeNumber(data[0]);
            Y = numberEncoderService.DecodeNumber(data[1]);
            ID = (short) numberEncoderService.DecodeNumber(data[2], data[3]);
            SpawnType = (byte) numberEncoderService.DecodeNumber(data[4]);
            RespawnTime = (short) numberEncoderService.DecodeNumber(data[5], data[6]);
            Amount = (byte) numberEncoderService.DecodeNumber(data[7]);
        }
    }
}
