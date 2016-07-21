// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public class WarpMapEntity : IMapEntity
    {
        public int DataSize { get { return 8; } }

        public int X { get; private set; }

        public int Y { get; private set; }

        public short DestinationMapID { get; set; }

        public byte DestinationMapX { get; set; }

        public byte DestinationMapY { get; set; }

        public byte LevelRequirement { get; set; }

        public DoorSpec DoorType { get; set; }

        /// <summary>
        /// Construct a WarpMapEntity with the specified Y-coordinate.
        /// </summary>
        /// <remarks>This is necessary because warp elements are stored with a single y-coordinate per row instead of per element</remarks>
        internal WarpMapEntity(int yCoord)
        {
            Y = yCoord;
        }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                           IMapStringEncoderService mapStringEncoderService)
        {
            var retBytes = new List<byte>(DataSize);

            retBytes.AddRange(numberEncoderService.EncodeNumber(X, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(DestinationMapID, 2));
            retBytes.AddRange(numberEncoderService.EncodeNumber(DestinationMapX, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(DestinationMapY, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(LevelRequirement, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber((short)DoorType, 2));

            return retBytes.ToArray();
        }

        public void DeserializeFromByteArray(byte[] data,
                                             INumberEncoderService numberEncoderService,
                                             IMapStringEncoderService mapStringEncoderService)
        {
            if (data.Length != DataSize)
                throw new ArgumentException("Data is improperly sized for deserialization", "data");

            X = numberEncoderService.DecodeNumber(data[0]);
            DestinationMapID = (short)numberEncoderService.DecodeNumber(data[1], data[2]);
            DestinationMapX = (byte)numberEncoderService.DecodeNumber(data[3]);
            DestinationMapY = (byte)numberEncoderService.DecodeNumber(data[4]);
            LevelRequirement = (byte)numberEncoderService.DecodeNumber(data[5]);
            DoorType = (DoorSpec)numberEncoderService.DecodeNumber(data[6], data[7]);
        }
    }
}
