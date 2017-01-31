// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using EOLib.Net.Translators;

namespace EOLib.PacketHandlers
{
    public class PlayerEnterMapHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _mapStateRepository;
        private readonly ICharacterFromPacketFactory _characterFromPacketFactory;

        public override PacketFamily Family { get { return PacketFamily.Players; } }

        public override PacketAction Action { get { return PacketAction.Agree; } }

        public PlayerEnterMapHandler(IPlayerInfoProvider playerInfoProvider,
                                     ICurrentMapStateRepository mapStateRepository,
                                     ICharacterFromPacketFactory characterFromPacketFactory)
            : base(playerInfoProvider)
        {
            _mapStateRepository = mapStateRepository;
            _characterFromPacketFactory = characterFromPacketFactory;
        }

        public override bool HandlePacket(IPacket packet)
        {
            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing 255 header byte for player enter map handler", packet);

            var character = _characterFromPacketFactory.CreateCharacter(packet);

            var anim = WarpAnimation.None;
            if (packet.PeekByte() != 255) //next byte was the warp animation: sent on Map::Enter in eoserv
            {
                //todo: need to signal client that animation should be performed
                anim = (WarpAnimation) packet.ReadChar();
                if (packet.ReadByte() != 255) //the 255 still needs to be read...
                    throw new MalformedPacketException("Missing 255 byte after the warp animation for player enter map handler", packet);
            }
            else //next byte was a 255. Read it and proceed normally.
            {
                packet.ReadByte();
            }

            //0 for NPC, 1 for player. In eoserv it is never 0.
            if (packet.ReadChar() != 1)
                throw new MalformedPacketException(
                    "Missing '1' char after warp animation for player enter map handler. Are you using a non-standard version of EOSERV?",
                    packet);

            var existingCharacter = _mapStateRepository.Characters.SingleOrDefault(x => x.ID == character.ID);
            if (existingCharacter != null)
            {
                //note: this was taken from OldCharacter.ApplyData (before function was removed)
                var existingRenderProps = existingCharacter.RenderProperties;
                var newRenderProps = existingRenderProps
                    .WithBootsGraphic(character.RenderProperties.BootsGraphic)
                    .WithArmorGraphic(character.RenderProperties.ArmorGraphic)
                    .WithHatGraphic(character.RenderProperties.HatGraphic)
                    .WithShieldGraphic(character.RenderProperties.ShieldGraphic)
                    .WithWeaponGraphic(character.RenderProperties.WeaponGraphic)
                    .WithDirection(character.RenderProperties.Direction)
                    .WithHairStyle(character.RenderProperties.HairStyle)
                    .WithHairColor(character.RenderProperties.HairColor)
                    .WithGender(character.RenderProperties.Gender)
                    .WithRace(character.RenderProperties.Race)
                    .WithSitState(character.RenderProperties.SitState)
                    .WithMapX(character.RenderProperties.MapX)
                    .WithMapY(character.RenderProperties.MapY)
                    .ResetAnimationFrames();

                var existingStats = existingCharacter.Stats;
                var newStats = existingStats
                    .WithNewStat(CharacterStat.Level, existingStats[CharacterStat.Level])
                    .WithNewStat(CharacterStat.HP, existingStats[CharacterStat.HP])
                    .WithNewStat(CharacterStat.MaxHP, existingStats[CharacterStat.MaxHP])
                    .WithNewStat(CharacterStat.TP, existingStats[CharacterStat.TP])
                    .WithNewStat(CharacterStat.MaxTP, existingStats[CharacterStat.MaxTP]);

                character = existingCharacter
                    .WithName(character.Name)
                    .WithGuildTag(character.GuildTag)
                    .WithMapID(character.MapID)
                    .WithRenderProperties(newRenderProps)
                    .WithStats(newStats);

                _mapStateRepository.Characters.Remove(existingCharacter);
            }
            _mapStateRepository.Characters.Add(character);

            return true;
        }
    }
}
