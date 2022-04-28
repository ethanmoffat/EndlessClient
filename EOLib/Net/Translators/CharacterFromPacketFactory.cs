using System.IO;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;

namespace EOLib.Net.Translators
{
    [AutoMappedType]
    public class CharacterFromPacketFactory : ICharacterFromPacketFactory
    {
        private readonly IEIFFileProvider _eifFileProvider;

        public CharacterFromPacketFactory(IEIFFileProvider eifFileProvider)
        {
            _eifFileProvider = eifFileProvider;
        }

        public Character CreateCharacter(IPacket packet)
        {
            var name = packet.ReadBreakString();
            name = char.ToUpper(name[0]) + name.Substring(1);

            var id = packet.ReadShort();
            var mapID = packet.ReadShort();
            var xLoc = packet.ReadShort();
            var yLoc = packet.ReadShort();

            var direction = (EODirection)packet.ReadChar();
            var classID = packet.ReadChar();
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

            var renderProps = new CharacterRenderProperties.Builder
            { 
               Direction = direction,
               Gender = gender,
               HairStyle = hairStyle,
               HairColor = hairColor,
               Race = race,
               BootsGraphic = boots,
               ArmorGraphic = armor,
               HatGraphic = hat,
               ShieldGraphic = shield,
               WeaponGraphic = weapon,
               IsRangedWeapon = _eifFileProvider.EIFFile.IsRangedWeapon(weapon),
               SitState = sitState,
               CurrentAction = sitState == SitState.Standing ? CharacterActionState.Standing : CharacterActionState.Sitting,
               IsHidden = hidden,
               MapX = xLoc,
               MapY = yLoc,
            };

            return new Character.Builder
            { 
                Name = name,
                ID = id,
                ClassID = classID,
                MapID = mapID,
                GuildTag = guildTag,
                Stats = stats,
                RenderProperties = renderProps.ToImmutable(),
            }.ToImmutable();
        }
    }

    public interface ICharacterFromPacketFactory
    {
        Character CreateCharacter(IPacket packet);
    }
}
