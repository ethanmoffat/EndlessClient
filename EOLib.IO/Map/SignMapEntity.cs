// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EOLib.IO.Services;

namespace EOLib.IO.Map
{
    public class SignMapEntity : IMapEntity
    {
        public int DataSize { get { return 5; } }

        public int X { get; private set; }

        public int Y { get; private set; }

        public string Title { get; private set; }

        public string Message { get; private set; }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService,
                                           IMapStringEncoderService mapStringEncoderService)
        {
            var retBytes = new List<byte>(DataSize + Title.Length + Message.Length);

            retBytes.AddRange(numberEncoderService.EncodeNumber(X, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(Y, 1));
            retBytes.AddRange(numberEncoderService.EncodeNumber(Title.Length + Message.Length + 1, 2));

            var fileMsg = new byte[Message.Length + Title.Length];

            var rawTitle = mapStringEncoderService.EncodeMapString(Title);
            Array.Copy(rawTitle, fileMsg, fileMsg.Length);

            var rawMessage = mapStringEncoderService.EncodeMapString(Message);
            Array.Copy(rawMessage, 0, fileMsg, rawTitle.Length, rawMessage.Length);

            retBytes.AddRange(fileMsg);
            retBytes.AddRange(numberEncoderService.EncodeNumber(rawTitle.Length, 1));

            return retBytes.ToArray();
        }

        public void DeserializeFromByteArray(byte[] data,
                                             INumberEncoderService numberEncoderService,
                                             IMapStringEncoderService mapStringEncoderService)
        {
            if (data.Length < DataSize) //using < for comparison because data will be an unknown length based on message
                throw new ArgumentException("Data is improperly size for serialization", "data");

            X = numberEncoderService.DecodeNumber(data[0]);
            Y = numberEncoderService.DecodeNumber(data[1]);

            var titleAndMessageLength = numberEncoderService.DecodeNumber(data[2], data[3]) - 1;
            if (data.Length != DataSize + titleAndMessageLength)
                throw new ArgumentException("Data is improperly sized for deserialization", "data");

            var rawTitleAndMessage = data.Skip(4).Take(titleAndMessageLength).ToArray();
            var titleLength = numberEncoderService.DecodeNumber(data[4 + titleAndMessageLength]);

            var titleAndMessage = mapStringEncoderService.DecodeMapString(rawTitleAndMessage);
            Title = titleAndMessage.Substring(0, titleLength);
            Message = titleAndMessage.Substring(titleLength);
        }
    }
}
