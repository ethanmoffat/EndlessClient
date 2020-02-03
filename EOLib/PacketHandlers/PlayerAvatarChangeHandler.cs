// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
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
        private readonly IEIFFileProvider _eifFileProvider;

        public override PacketFamily Family => PacketFamily.Avatar;

        public override PacketAction Action => PacketAction.Agree;

        public PlayerAvatarChangeHandler(IPlayerInfoProvider playerInfoProvider,
                                         ICurrentMapStateRepository currentMapStateRepository,
                                         IEIFFileProvider eifFileProvider)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _eifFileProvider = eifFileProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var playerID = packet.ReadShort();
            ICharacter currentCharacter;
            try
            {
                currentCharacter = _currentMapStateRepository.Characters.Single(x => x.ID == playerID);
            }
            catch (InvalidOperationException) { return false; }

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
            _currentMapStateRepository.Characters.Remove(currentCharacter);
            _currentMapStateRepository.Characters.Add(updatedCharacter);

            return true;
        }
    }
}
