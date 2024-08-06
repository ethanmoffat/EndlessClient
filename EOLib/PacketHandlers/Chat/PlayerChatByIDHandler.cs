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
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterProvider _characterProvider;

        public override PacketFamily Family => PacketFamily.Talk;

        protected PlayerChatByIDHandler(IPlayerInfoProvider playerInfoProvider,
                                        ICurrentMapStateRepository currentMapStateRepository,
                                        ICharacterProvider characterProvider)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterProvider = characterProvider;
        }

        protected bool Handle(TPacket packet, int fromPlayerID)
        {
            if (!_currentMapStateRepository.Characters.TryGetValue(fromPlayerID, out var character) &&
                _characterProvider.MainCharacter.ID != fromPlayerID)
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(fromPlayerID);
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
