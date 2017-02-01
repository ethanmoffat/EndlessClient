// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.IO;
using EOLib.Domain.Protocol;

namespace EOLib.Net.Translators
{
    public class InitDataTranslator : IPacketTranslator<IInitializationData>
    {
        public IInitializationData TranslatePacket(IPacket packet)
        {
            var response = (InitReply) packet.ReadByte();
            switch (response)
            {
                case InitReply.BannedFromServer: return GetInitializationBannedData(packet);
                case InitReply.ClientOutOfDate: return GetInitializationOutOfDateData(packet);
                case InitReply.Success: return GetInitializationSuccessData(packet);
                default: throw new InvalidInitResponseException(response);
            }
        }

        private IInitializationData GetInitializationBannedData(IPacket packet)
        {
            var banType = (BanType)packet.ReadByte();
            byte banTimeRemaining = 0;
            if(banType == BanType.TemporaryBan)
                banTimeRemaining = packet.ReadByte();
            return new InitializationBannedData(banType, banTimeRemaining);
        }

        private IInitializationData GetInitializationOutOfDateData(IPacket packet)
        {
            packet.Seek(2, SeekOrigin.Current);
            return new InitializationOutOfDateData(packet.ReadChar());
        }

        private IInitializationData GetInitializationSuccessData(IPacket packet)
        {
            return new InitializationSuccessData(
                packet.ReadByte(),
                packet.ReadByte(),
                packet.ReadByte(),
                packet.ReadByte(),
                packet.ReadShort(),
                packet.ReadThree()
                );
        }
    }

    public class InvalidInitResponseException : Exception
    {
        public InvalidInitResponseException(InitReply reply)
            : base($"Invalid InitReply from server: {reply}") { }
    }
}
