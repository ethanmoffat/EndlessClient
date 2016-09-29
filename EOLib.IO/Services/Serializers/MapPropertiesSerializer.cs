// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    public class MapPropertiesSerializer : ISerializer<IMapFileProperties>
    {
        private readonly INumberEncoderService _numberEncoderService;
        private readonly IMapStringEncoderService _mapStringEncoderService;

        public MapPropertiesSerializer(INumberEncoderService numberEncoderService,
                                       IMapStringEncoderService mapStringEncoderService)
        {
            _numberEncoderService = numberEncoderService;
            _mapStringEncoderService = mapStringEncoderService;
        }

        public byte[] SerializeToByteArray(IMapFileProperties mapEntity)
        {
            var ret = new List<byte>();

            ret.AddRange(Encoding.ASCII.GetBytes(mapEntity.FileType));
            ret.AddRange(mapEntity.Checksum);

            var fullName = Enumerable.Repeat((byte)0xFF, 24).ToArray();
            var encodedName = _mapStringEncoderService.EncodeMapString(mapEntity.Name);
            Array.Copy(encodedName, 0, fullName, fullName.Length - encodedName.Length, encodedName.Length);
            ret.AddRange(fullName);

            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.PKAvailable ? 3 : 0, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber((byte)mapEntity.Effect, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Music, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.MusicExtra, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.AmbientNoise, 2));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Width, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Height, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.FillTile, 2));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.MapAvailable ? 1 : 0, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.CanScroll ? 1 : 0, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.RelogX, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.RelogY, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Unknown2, 1));

            return ret.ToArray();
        }

        public IMapFileProperties DeserializeFromByteArray(byte[] data)
        {
            IMapFileProperties properties = new MapFileProperties();
            if (data.Length != MapFileProperties.DATA_SIZE)
                throw new ArgumentException("Data is not sized correctly for proper deserialization", "data");

            var typeString = Encoding.ASCII.GetString(data.Take(3).ToArray());
            if (typeString != properties.FileType)
                throw new FormatException("Data is not correctly formatted! Must be an EMF file header");

            var checksumArray = data.Skip(7).Take(24).ToArray();

            properties = properties.WithChecksum(data.Skip(3).Take(4).ToArray())
                .WithName(_mapStringEncoderService.DecodeMapString(checksumArray))
                .WithPKAvailable(_numberEncoderService.DecodeNumber(data[31]) == 3 ||
                                 (checksumArray[0] == 0xFF && checksumArray[1] == 0x01))
                .WithEffect((MapEffect) _numberEncoderService.DecodeNumber(data[32]))
                .WithMusic((byte) _numberEncoderService.DecodeNumber(data[33]))
                .WithMusicExtra((byte) _numberEncoderService.DecodeNumber(data[34]))
                .WithAmbientNoise((short) _numberEncoderService.DecodeNumber(data[35], data[36]))
                .WithWidth((byte) _numberEncoderService.DecodeNumber(data[37]))
                .WithHeight((byte) _numberEncoderService.DecodeNumber(data[38]))
                .WithFillTile((short) _numberEncoderService.DecodeNumber(data[39], data[40]))
                .WithMapAvailable(_numberEncoderService.DecodeNumber(data[41]) == 1)
                .WithScrollAvailable(_numberEncoderService.DecodeNumber(data[42]) == 1)
                .WithRelogX((byte) _numberEncoderService.DecodeNumber(data[43]))
                .WithRelogY((byte) _numberEncoderService.DecodeNumber(data[44]))
                .WithUnknown2((byte) _numberEncoderService.DecodeNumber(data[45]));

            return properties;
        }
    }
}
