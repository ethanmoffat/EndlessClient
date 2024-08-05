using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.PacketHandlers.Chat
{
    public abstract class PlayerChatByNameBase<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        public override PacketFamily Family => PacketFamily.Talk;

        protected PlayerChatByNameBase(IPlayerInfoProvider playerInfoProvider)
            : base(playerInfoProvider) { }

        protected bool Handle(string name, string message)
        {
            name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
            PostChat(name, message);

            return true;
        }

        protected abstract void PostChat(string name, string message);
    }
}