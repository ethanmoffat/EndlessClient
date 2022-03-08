using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class PlayerAvatarChangeHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEIFFileProvider _eifFileProvider;

        public override PacketFamily Family => PacketFamily.Avatar;

        public override PacketAction Action => PacketAction.Agree;

        public PlayerAvatarChangeHandler(IPlayerInfoProvider playerInfoProvider,
                                         ICurrentMapStateRepository currentMapStateRepository,
                                         ICharacterRepository characterRepository,
                                         IEIFFileProvider eifFileProvider)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _characterRepository = characterRepository;
            _eifFileProvider = eifFileProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerID = packet.ReadShort();
            ICharacter currentCharacter;

            if (_characterRepository.MainCharacter.ID == playerID)
            {
                currentCharacter = _characterRepository.MainCharacter;
            }
            else if (_currentMapStateRepository.Characters.ContainsKey(playerID))
            {
                currentCharacter = _currentMapStateRepository.Characters[playerID];
            }
            else
            {
                return false;
            }

            var currentRenderProps = currentCharacter.RenderProperties;

            var slot = (AvatarSlot) packet.ReadChar();
            switch (slot)
            {
                case AvatarSlot.Clothes:
                {
                    var sound = packet.ReadChar() == 0; //todo: sound?

                    currentRenderProps = currentRenderProps
                        .WithBootsGraphic(packet.ReadShort())
                        .WithArmorGraphic(packet.ReadShort())
                        .WithHatGraphic(packet.ReadShort());

                    var weaponGraphic = packet.ReadShort();
                    currentRenderProps = currentRenderProps
                        .WithWeaponGraphic(weaponGraphic, _eifFileProvider.EIFFile.IsRangedWeapon(weaponGraphic))
                        .WithShieldGraphic(packet.ReadShort());

                    break;
                }
                case AvatarSlot.Hair:
                {
                    if (packet.ReadChar() != 0) //subloc -- not sure what this does
                        throw new MalformedPacketException("Missing expected 0 byte in updating hair packet", packet);

                    currentRenderProps = currentRenderProps
                        .WithHairStyle(packet.ReadChar())
                        .WithHairColor(packet.ReadChar());

                    break;
                }
                case AvatarSlot.HairColor:
                {
                    if (packet.ReadChar() != 0) //subloc -- not sure what this does
                        throw new MalformedPacketException("Missing expected 0 byte in updating hair color packet", packet);

                    currentRenderProps = currentRenderProps
                        .WithHairColor(packet.ReadChar());

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
                _currentMapStateRepository.Characters[playerID] = updatedCharacter;
            }

            return true;
        }
    }
}
