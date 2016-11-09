// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    public class PlayerDirectionHandler : IPacketHandler
    {
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ICurrentMapStateRepository _mapStateRepository;

        public PacketFamily Family { get { return PacketFamily.Face; } }

        public PacketAction Action { get { return PacketAction.Player; } }

        public bool CanHandle { get { return _playerInfoProvider.PlayerIsInGame; } }

        public PlayerDirectionHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICurrentMapStateRepository mapStateRepository)
        {
            _playerInfoProvider = playerInfoProvider;
            _mapStateRepository = mapStateRepository;
        }

        public bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();
            var direction = (EODirection) packet.ReadChar();

            ICharacter character;
            try
            {
                character = _mapStateRepository.Characters.Single(x => x.ID == id);
            }
            catch (InvalidOperationException) { return false; } //more than 1 character with that ID - thrown by .Single()

            var newRenderProps = character.RenderProperties.WithDirection(direction);
            var newCharacter = character.WithRenderProperties(newRenderProps);

            _mapStateRepository.Characters.Remove(character);
            _mapStateRepository.Characters.Add(newCharacter);

            return true;
        }

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }
    }
}
