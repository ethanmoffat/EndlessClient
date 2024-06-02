using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;
using EOLib.Domain.Interact.Barber;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;

namespace EOLib.PacketHandlers.Barber
{
    [AutoMappedType]
    public class BarberAgreeHandler : InGameOnlyPacketHandler
    {
        private readonly IBarberDataRepository _barberDataRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Barber;
        public override PacketAction Action => PacketAction.Agree;

        public BarberAgreeHandler(
            IPlayerInfoProvider playerInfoProvider,
            IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers,
            IBarberDataRepository barberDataRepository,
            ICharacterRepository characterRepository,
            ICurrentMapStateRepository currentMapStateRepository,
            ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _npcInteractionNotifiers = npcInteractionNotifiers;
            _barberDataRepository = barberDataRepository;
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var amount = packet.ReadInt();
            var gold = new InventoryItem(1, amount);
            var playerID = packet.ReadShort();

            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(gold);

            var currentCharacter = _characterRepository.MainCharacter.ID == playerID
                ? _characterRepository.MainCharacter
                : null;

            if (currentCharacter == null)
            {
                return false;
            }

            var currentRenderProps = currentCharacter.RenderProperties;
            var slot = (AvatarSlot)packet.ReadChar();

            switch (slot)
            {
                case AvatarSlot.Hair:
                    if (packet.ReadChar() != 0)
                        throw new MalformedPacketException("Missing expected 0 byte in updating hair packet", packet);

                    currentRenderProps = currentRenderProps
                        .WithHairStyle(packet.ReadChar())
                        .WithHairColor(packet.ReadChar());
                    break;

                case AvatarSlot.HairColor:
                    if (packet.ReadChar() != 0)
                        throw new MalformedPacketException("Missing expected 0 byte in updating hair color packet", packet);

                    currentRenderProps = currentRenderProps
                        .WithHairColor(packet.ReadChar());
                    break;
            }

            var updatedCharacter = currentCharacter.WithRenderProperties(currentRenderProps);

            if (_characterRepository.MainCharacter.ID == playerID)
            {
                _characterRepository.MainCharacter = updatedCharacter;
            }
            else
            {
                _currentMapStateRepository.Characters.Update(currentCharacter, updatedCharacter);
            }

            return true;
        }
    }
}
