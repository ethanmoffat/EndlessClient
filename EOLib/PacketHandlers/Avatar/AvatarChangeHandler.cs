using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Avatar
{
    public abstract class AvatarChangeHandler<TPacket> : InGameOnlyPacketHandler<TPacket>
        where TPacket : IPacket
    {
        protected readonly ICurrentMapStateRepository _currentMapStateRepository;
        protected readonly ICharacterRepository _characterRepository;

        public AvatarChangeHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterRepository = characterRepository;
        }

        protected void Handle(AvatarChange change)
        {
            var playerID = change.PlayerId;
            Character currentCharacter;

            if (_characterRepository.MainCharacter.ID == playerID)
            {
                currentCharacter = _characterRepository.MainCharacter;
            }
            else if (!_currentMapStateRepository.Characters.TryGetValue(playerID, out currentCharacter))
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerID);
                return;
            }

            var currentRenderProps = currentCharacter.RenderProperties;

            switch (change.ChangeType)
            {
                case AvatarChangeType.Equipment:
                    {
                        var data = (AvatarChange.ChangeTypeDataEquipment)change.ChangeTypeData;
                        currentRenderProps = currentRenderProps
                            .WithBootsGraphic(data.Equipment.Boots)
                            .WithArmorGraphic(data.Equipment.Armor)
                            .WithHatGraphic(data.Equipment.Hat)
                            .WithWeaponGraphic(data.Equipment.Weapon)
                            .WithShieldGraphic(data.Equipment.Shield);
                        break;
                    }
                case AvatarChangeType.Hair:
                    {
                        var data = (AvatarChange.ChangeTypeDataHair)change.ChangeTypeData;
                        currentRenderProps = currentRenderProps
                            .WithHairStyle(data.HairStyle)
                            .WithHairColor(data.HairColor);
                        break;
                    }
                case AvatarChangeType.HairColor:
                    {
                        var data = (AvatarChange.ChangeTypeDataHairColor)change.ChangeTypeData;
                        currentRenderProps = currentRenderProps.WithHairColor(data.HairColor);
                        break;
                    }
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
        }
    }
}