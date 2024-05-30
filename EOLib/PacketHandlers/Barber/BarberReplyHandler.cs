using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;
using System.Diagnostics;
using EOLib.Domain.Interact.Barber;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;

namespace EOLib.PacketHandlers.Barber
{
    [AutoMappedType]
    public class BarberReplyHandler : InGameOnlyPacketHandler
    {

        private const byte BuySellSfxId = 26;
        private readonly IBarberDataRepository _barberDataRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;
        protected readonly ICharacterRepository _characterRepository;
        protected readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<ISoundNotifier> _soundNotifiers;


        public override PacketFamily Family => PacketFamily.Barber;
        public override PacketAction Action => PacketAction.Agree;

        public BarberReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                  IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers,
                                  IBarberDataRepository barberDataRepository,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  ICharacterInventoryRepository characterInventoryRepository,
                                  IEnumerable<ISoundNotifier> soundNotifiers)
            : base(playerInfoProvider)
        {
            _npcInteractionNotifiers = npcInteractionNotifiers;
            _barberDataRepository = barberDataRepository;
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _soundNotifiers = soundNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var amount = packet.ReadInt();
            var gold = new InventoryItem(1, amount);
           
            var playerID = packet.ReadShort();

            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(gold);

            Character currentCharacter = _characterRepository.MainCharacter.ID == playerID 
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
                    if (packet.ReadChar() != 0) //subloc -- not sure what this does
                        throw new MalformedPacketException("Missing expected 0 byte in updating hair packet", packet);

                    currentRenderProps = currentRenderProps
                        .WithHairStyle(packet.ReadChar())
                        .WithHairColor(packet.ReadChar());
                    break;

                case AvatarSlot.HairColor:
                    if (packet.ReadChar() != 0) //subloc -- not sure what this does
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

            foreach (var notifier in _soundNotifiers)
                notifier.NotifySoundEffect(BuySellSfxId);

            return true;
        }
    }
}