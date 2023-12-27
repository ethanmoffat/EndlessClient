using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Chat
{
    public abstract class PlayerChatByIDHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterProvider _characterProvider;

        public override PacketFamily Family => PacketFamily.Talk;

        protected PlayerChatByIDHandler(IPlayerInfoProvider playerInfoProvider,
                                        ICurrentMapStateProvider currentMapStateProvider,
                                        ICharacterProvider characterProvider)
            : base(playerInfoProvider)
        {
            _currentMapStateProvider = currentMapStateProvider;
            _characterProvider = characterProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var fromPlayerID = packet.ReadShort();
            if (!_currentMapStateProvider.Characters.TryGetValue(fromPlayerID, out var character) &&
                _characterProvider.MainCharacter.ID != fromPlayerID)
            {
                _currentMapStateProvider.UnknownPlayerIDs.Add(fromPlayerID);
                return true;
            }

            if (_characterProvider.MainCharacter.ID == fromPlayerID)
                character = _characterProvider.MainCharacter;

            DoTalk(packet, character);

            return true;
        }

        protected abstract void DoTalk(IPacket packet, Character character);
    }
}
