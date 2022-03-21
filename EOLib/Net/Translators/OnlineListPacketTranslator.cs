using AutomaticTypeMapper;
using EOLib.Domain.Online;
using EOLib.Domain.Protocol;
using EOLib.IO.Repositories;
using System.Collections.Generic;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class OnlineListPacketTranslator : IPacketTranslator<IOnlineListData>
    {
        private readonly IECFFileProvider _classFileProvider;

        public OnlineListPacketTranslator(IECFFileProvider classFileProvider)
        {
            _classFileProvider = classFileProvider;
        }

        public IOnlineListData TranslatePacket(IPacket packet)
        {
            var reply = (InitReply)packet.ReadChar();

            if (reply != InitReply.AllPlayersList && reply != InitReply.FriendPlayersList)
                throw new MalformedPacketException($"Expected online list or friend list init data, but was {reply}", packet);

            short numTotal = packet.ReadShort();
            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Expected break byte after number of entries", packet);

            var retList = new List<OnlinePlayerInfo>(numTotal);
            for (int i = 0; i < numTotal; ++i)
            {
                string name = packet.ReadBreakString();

                if (reply == InitReply.AllPlayersList)
                {
                    var title = packet.ReadBreakString();
                    if (packet.ReadChar() != 0)
                        throw new MalformedPacketException("Expected 0 char after online entry title", packet);

                    var iconType = (OnlineIcon)packet.ReadChar();
                    int clsId = packet.ReadChar();
                    var guild = packet.ReadBreakString();

                    name = char.ToUpper(name[0]) + name.Substring(1);

                    if (string.IsNullOrWhiteSpace(title))
                        title = "-";
                    else
                        title = char.ToUpper(title[0]) + title.Substring(1);

                    var className = _classFileProvider.ECFFile.Length >= clsId
                        ? _classFileProvider.ECFFile[clsId].Name
                        : "-";

                    if (string.IsNullOrWhiteSpace(guild))
                        guild = "-";

                    retList.Add(new OnlinePlayerInfo(name, title, guild, className, iconType));
                }
                else
                {
                    retList.Add(new OnlinePlayerInfo(name));
                }
            }

            return new OnlineListData(retList);
        }
    }
}
