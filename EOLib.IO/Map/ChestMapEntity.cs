// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public class ChestMapEntity : IMapEntity
    {
        public int DataSize { get { return 12; } }

        public int X { get; private set; }

        public int Y { get; private set; }

        public ChestKey Key { get; private set; }

        public byte Slot { get; private set; }

        public short ItemID { get; private set; }

        public short RespawnTime { get; private set; }

        public int Amount { get; private set; }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService)
        {
            var retBytes = new List<byte>(12);

            retBytes.AddRange(numberEncoderService.EncodeNumber(X, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(Y, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber((short)Key, 2));
            retBytes.AddRange(numberEncoderService.EncodeNumber(Slot, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(ItemID, 2));
            retBytes.AddRange(numberEncoderService.EncodeNumber(RespawnTime, 2));
            retBytes.AddRange(numberEncoderService.EncodeNumber(Amount, 3));

            return retBytes.ToArray();
        }

        public void DeserializeFromByteArray(byte[] data, INumberEncoderService numberEncoderService)
        {
            if (data.Length != DataSize)
                throw new ArgumentException("Data is improperly size for serialization", "data");

            X = numberEncoderService.DecodeNumber(data[0]);
            Y = numberEncoderService.DecodeNumber(data[1]);
            Key = (ChestKey) numberEncoderService.DecodeNumber(data[2], data[3]);
            Slot = (byte) numberEncoderService.DecodeNumber(data[4]);
            ItemID = (short) numberEncoderService.DecodeNumber(data[5], data[6]);
            RespawnTime = (short) numberEncoderService.DecodeNumber(data[7], data[8]);
            Amount = numberEncoderService.DecodeNumber(data[9], data[10], data[11]);
        }
    }
}
