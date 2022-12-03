using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Avatar
{
    /// <summary>
    /// Sent when a character's render properties are changed
    /// </summary>
    [AutoMappedType]
    public class AvatarAgreeHandler : InGameOnlyPacketHandler
    {
        protected readonly ICurrentMapStateRepository _currentMapStateRepository;
        protected readonly ICharacterRepository _characterRepository;
        protected readonly IEIFFileProvider _eifFileProvider;

        public override PacketFamily Family => PacketFamily.Avatar;

        public override PacketAction Action => PacketAction.Agree;

        public AvatarAgreeHandler(IPlayerInfoProvider playerInfoProvider,
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
            Character currentCharacter;

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
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerID);
                return true;
            }

            var currentRenderProps = currentCharacter.RenderProperties;

            var slot = (AvatarSlot)packet.ReadChar();
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
                            .WithWeaponGraphic(weaponGraphic)
                            .WithIsRangedWeapon(_eifFileProvider.EIFFile.IsRangedWeapon(weaponGraphic))
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
