// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class CharacterFromPacketFactory : ICharacterFromPacketFactory
    {
        public ICharacter CreateCharacter(IPacket packet)
        {
            var name = packet.ReadBreakString();
            name = char.ToUpper(name[0]) + name.Substring(1);

            var id = packet.ReadShort();
            var mapID = packet.ReadShort();
            var xLoc = packet.ReadShort();
            var yLoc = packet.ReadShort();

            var direction = (EODirection)packet.ReadChar();
            packet.ReadChar(); //value is always 6? Unknown use
            var guildTag = packet.ReadString(3);

            var level = packet.ReadChar();
            var gender = packet.ReadChar();
            var hairStyle = packet.ReadChar();
            var hairColor = packet.ReadChar();
            var race = packet.ReadChar();

            var maxHP = packet.ReadShort();
            var hp = packet.ReadShort();
            var maxTP = packet.ReadShort();
            var tp = packet.ReadShort();

            var boots = packet.ReadShort();
            packet.Seek(6, SeekOrigin.Current); //0s
            var armor = packet.ReadShort();
            packet.Seek(2, SeekOrigin.Current); //0
            var hat = packet.ReadShort();
            var shield = packet.ReadShort();
            var weapon = packet.ReadShort();

            var sitState = (SitState)packet.ReadChar();
            var hidden = packet.ReadChar() != 0;

            var stats = new CharacterStats()
                .WithNewStat(CharacterStat.Level, level)
                .WithNewStat(CharacterStat.HP, hp)
                .WithNewStat(CharacterStat.MaxHP, maxHP)
                .WithNewStat(CharacterStat.TP, tp)
                .WithNewStat(CharacterStat.MaxTP, maxTP);

            var renderProps = new CharacterRenderProperties()
                .WithDirection(direction)
                .WithGender(gender)
                .WithHairStyle(hairStyle)
                .WithHairColor(hairColor)
                .WithRace(race)
                .WithBootsGraphic(boots)
                .WithArmorGraphic(armor)
                .WithHatGraphic(hat)
                .WithShieldGraphic(shield)
                .WithWeaponGraphic(weapon)
                .WithSitState(sitState)
                .WithIsHidden(hidden)
                .WithMapX(xLoc)
                .WithMapY(yLoc);

            return new Character()
                .WithName(name)
                .WithID(id)
                .WithMapID(mapID)
                .WithGuildTag(guildTag)
                .WithStats(stats)
                .WithRenderProperties(renderProps);
        }
    }

    public interface ICharacterFromPacketFactory
    {
        ICharacter CreateCharacter(IPacket packet);
    }
}
