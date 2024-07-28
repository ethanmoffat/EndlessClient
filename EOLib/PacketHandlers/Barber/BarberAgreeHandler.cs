using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Barber
{
    [AutoMappedType]
    public class BarberAgreeHandler : InGameOnlyPacketHandler<BarberAgreeServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Barber;

        public override PacketAction Action => PacketAction.Agree;

        public BarberAgreeHandler(
            IPlayerInfoProvider playerInfoProvider,
            ICharacterRepository characterRepository,
            ICurrentMapStateRepository currentMapStateRepository,
            ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(BarberAgreeServerPacket packet)
        {
            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, packet.GoldAmount));

            if (_characterRepository.MainCharacter.ID == packet.Change.PlayerId)
            {
                var currentCharacter = _characterRepository.MainCharacter;
                _characterRepository.MainCharacter = currentCharacter.WithRenderProperties(UpdateRenderProperties(currentCharacter, packet));
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(packet.Change.PlayerId))
            {
                var currentCharacter = _currentMapStateRepository.Characters[packet.Change.PlayerId];
                _currentMapStateRepository.Characters.Update(currentCharacter, currentCharacter.WithRenderProperties(UpdateRenderProperties(currentCharacter, packet)));
            }

            return true;
        }

        private CharacterRenderProperties UpdateRenderProperties(Character currentCharacter, BarberAgreeServerPacket packet)
        {
            var currentRenderProps = currentCharacter.RenderProperties;
            switch (packet.Change.ChangeType)
            {
                case AvatarChangeType.Hair:
                    {
                        var data = (AvatarChange.ChangeTypeDataHair)packet.Change.ChangeTypeData;
                        currentRenderProps = currentRenderProps
                            .WithHairStyle(data.HairStyle)
                            .WithHairColor(data.HairColor);
                    }
                    break;
                case AvatarChangeType.HairColor:
                    {
                        var data = (AvatarChange.ChangeTypeDataHairColor)packet.Change.ChangeTypeData;
                        currentRenderProps = currentRenderProps
                            .WithHairColor(data.HairColor);
                    }
                    break;
            }
            return currentRenderProps;
        }
    }
}