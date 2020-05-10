using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public abstract class PlayerChatByNameBase : InGameOnlyPacketHandler
    {
        public override PacketFamily Family => PacketFamily.Talk;

        protected PlayerChatByNameBase(IPlayerInfoProvider playerInfoProvider)
            : base(playerInfoProvider) { }

        public override bool HandlePacket(IPacket packet)
        {
            var name = packet.ReadBreakString();
            var message = packet.ReadBreakString();

            name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
            PostChat(name, message);

            return true;
        }

        protected abstract void PostChat(string name, string message);
    }
}
