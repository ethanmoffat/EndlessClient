// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.IO.Map;

namespace EOLib.IO.Services.Serializers
{
    public class SignMapEntitySerializer : IMapEntitySerializer<SignMapEntity>
    {
        private readonly INumberEncoderService numberEncoderService;
        private readonly IMapStringEncoderService mapStringEncoderService;

        public int DataSize { get { return 5; } }

        public MapEntitySerializeType MapEntitySerializeType
        {
            get { return MapEntitySerializeType.MapSignEntitySerializer; }
        }

        public SignMapEntitySerializer(INumberEncoderService numberEncoderService,
                                       IMapStringEncoderService mapStringEncoderService)
        {
            this.numberEncoderService = numberEncoderService;
            this.mapStringEncoderService = mapStringEncoderService;
        }

        public byte[] SerializeToByteArray(SignMapEntity mapEntity)
        {
            var retBytes = new List<byte>(DataSize + mapEntity.Title.Length + mapEntity.Message.Length);

            retBytes.AddRange(numberEncoderService.EncodeNumber(mapEntity.X, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(mapEntity.Y, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(mapEntity.Title.Length + mapEntity.Message.Length + 1, 2));

            var fileMsg = new byte[mapEntity.Message.Length + mapEntity.Title.Length];

            var rawTitle = mapStringEncoderService.EncodeMapString(mapEntity.Title);
            Array.Copy(rawTitle, fileMsg, fileMsg.Length);

            var rawMessage = mapStringEncoderService.EncodeMapString(mapEntity.Message);
            Array.Copy(rawMessage, 0, fileMsg, rawTitle.Length, rawMessage.Length);

            retBytes.AddRange(fileMsg);
            retBytes.AddRange(numberEncoderService.EncodeNumber(rawTitle.Length, 1));

            return retBytes.ToArray();
        }

        public SignMapEntity DeserializeFromByteArray(byte[] data)
        {
            if (data.Length < DataSize) //using < for comparison because data will be an unknown length based on message
                throw new ArgumentException("Data is improperly size for serialization", "data");

            var sign = new SignMapEntity
            {
                X = numberEncoderService.DecodeNumber(data[0]),
                Y = numberEncoderService.DecodeNumber(data[1])
            };

            var titleAndMessageLength = numberEncoderService.DecodeNumber(data[2], data[3]) - 1;
            if (data.Length != DataSize + titleAndMessageLength)
                throw new ArgumentException("Data is improperly sized for deserialization", "data");

            var rawTitleAndMessage = data.Skip(4).Take(titleAndMessageLength).ToArray();
            var titleLength = numberEncoderService.DecodeNumber(data[4 + titleAndMessageLength]);

            var titleAndMessage = mapStringEncoderService.DecodeMapString(rawTitleAndMessage);
            sign.Title = titleAndMessage.Substring(0, titleLength);
            sign.Message = titleAndMessage.Substring(titleLength);

            return sign;
        }
    }
}
