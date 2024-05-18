using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.PacketHandlers.Chat
{
    public abstract class PlayerChatByIDHandler<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
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

        protected bool Handle(TPacket packet, int fromPlayerID)
        {
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

        protected abstract void DoTalk(TPacket packet, Character character);
    }
}
