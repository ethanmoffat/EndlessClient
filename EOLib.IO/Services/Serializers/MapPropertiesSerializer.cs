using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomaticTypeMapper;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    [MappedType(BaseType = typeof(IMapEntitySerializer<IMapFileProperties>))]
    [MappedType(BaseType = typeof(IMapDeserializer<IMapFileProperties>))]
    public class MapPropertiesSerializer : IMapEntitySerializer<IMapFileProperties>
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

            var mapNameBytes = EncodeMapName(mapEntity);
            ret.AddRange(mapNameBytes);

            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.PKAvailable ? 3 : 0, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber((byte)mapEntity.Effect, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber(mapEntity.Music, 1));
            ret.AddRange(_numberEncoderService.EncodeNumber((byte)mapEntity.Control, 1));
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
                throw new ArgumentException("Data is not sized correctly for proper deserialization", nameof(data));

            var typeString = Encoding.ASCII.GetString(data.Take(3).ToArray());
            if (typeString != properties.FileType)
                throw new FormatException("Data is not correctly formatted! Must be an EMF file header");

            var checksumArray = data.Skip(3).Take(4).ToArray();
            var mapNameArray = data.Skip(7).Take(24).ToArray();
            var mapName = _mapStringEncoderService.DecodeMapString(mapNameArray);

            properties = properties.WithChecksum(checksumArray)
                .WithName(mapName)
                .WithPKAvailable(_numberEncoderService.DecodeNumber(data[31]) == 3 ||
                                 (mapNameArray[0] == 0xFF && mapNameArray[1] == 0x01))
                .WithEffect((MapEffect) _numberEncoderService.DecodeNumber(data[32]))
                .WithMusic(_numberEncoderService.DecodeNumber(data[33]))
                .WithControl((MusicControl)_numberEncoderService.DecodeNumber(data[34]))
                .WithAmbientNoise(_numberEncoderService.DecodeNumber(data[35], data[36]))
                .WithWidth(_numberEncoderService.DecodeNumber(data[37]))
                .WithHeight(_numberEncoderService.DecodeNumber(data[38]))
                .WithFillTile(_numberEncoderService.DecodeNumber(data[39], data[40]))
                .WithMapAvailable(_numberEncoderService.DecodeNumber(data[41]) == 1)
                .WithScrollAvailable(_numberEncoderService.DecodeNumber(data[42]) == 1)
                .WithRelogX(_numberEncoderService.DecodeNumber(data[43]))
                .WithRelogY(_numberEncoderService.DecodeNumber(data[44]))
                .WithUnknown2(_numberEncoderService.DecodeNumber(data[45]));

            return properties;
        }

        private byte[] EncodeMapName(IMapFileProperties mapEntity)
        {
            //need to pad the map name with 0 bytes so that the 'flippy' is the correct state based on the length
            //0 bytes are converted to 255 bytes before being written to the file

            var padding = Enumerable.Repeat((byte)0, 24 - mapEntity.Name.Length).ToArray();
            var nameToEncode = $"{mapEntity.Name}{Encoding.ASCII.GetString(padding)}";

            var encodedName = _mapStringEncoderService.EncodeMapString(nameToEncode, nameToEncode.Length);
            var formattedName = encodedName.Select(x => x == 0 ? (byte)255 : x).ToArray();
            return formattedName;
        }
    }
}
