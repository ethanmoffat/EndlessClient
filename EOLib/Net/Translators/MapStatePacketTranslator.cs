// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.IO;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;

namespace EOLib.Net.Translators
{
    public abstract class MapStatePacketTranslator<T> : IPacketTranslator<T>
        where T : ITranslatedData
    {
        public abstract T TranslatePacket(IPacket packet);

        protected IEnumerable<ICharacter> GetCharacters(IPacket packet)
        {
            var numCharacters = packet.ReadChar();
            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing 255 byte after number of characters", packet);

            for (int i = 0; i < numCharacters; ++i)
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

                var sitState = (SitState) packet.ReadChar();
                var hidden = packet.ReadChar() != 0;

                if (packet.ReadByte() != 255)
                    throw new MalformedPacketException("Missing 255 byte after character", packet);

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
                    .WithIsHidden(hidden);

                yield return new Character()
                    .WithName(name)
                    .WithID(id)
                    .WithMapID(mapID)
                    .WithMapX(xLoc)
                    .WithMapY(yLoc)
                    .WithGuildTag(guildTag)
                    .WithStats(stats)
                    .WithRenderProperties(renderProps);
            }
        }

        protected IEnumerable<IMapNPC> GetNPCs(IPacket packet)
        {
            while (packet.PeekByte() != 255)
            {
                var index = packet.ReadChar();
                var id = packet.ReadShort();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var direction = (EODirection) packet.ReadChar();
                
                yield return new MapNPC(id, index)
                    .WithX(x)
                    .WithY(y)
                    .WithDirection(direction);
            }
            
            packet.ReadByte(); //consume the tail 255 byte that broke loop iteration
        }

        protected IEnumerable<IMapItem> GetMapItems(IPacket packet)
        {
            while (packet.ReadPosition < packet.Length)
            {
                var uid = packet.ReadShort();
                var itemID = packet.ReadShort();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var amount = packet.ReadThree();

                yield return new MapItem(uid, itemID, x, y).WithAmount(amount);
            }
        }
    }
}
