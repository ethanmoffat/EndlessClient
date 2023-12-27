using EOLib.Domain.Character;
using EOLib.IO.Repositories;
using System.Collections.Generic;

namespace EOLib.Net.Translators
{
    public abstract class CharacterDisplayPacketTranslator<T> : IPacketTranslator<T>
        where T : ITranslatedData
    {
        private readonly IEIFFileProvider _eifFileProvider;

        protected CharacterDisplayPacketTranslator(IEIFFileProvider eifFileProvider)
        {
            _eifFileProvider = eifFileProvider;
        }

        public abstract T TranslatePacket(IPacket packet);

        protected IEnumerable<Character> GetCharacters(IPacket packet)
        {
            var characters = new List<Character>();

            var numberOfCharacters = (int)packet.ReadChar();

            // Optional AddByte call. EOSERV sends either 1 or 2, but GameServer appears
            //   to not send it on character delete
            packet.ReadBreakString();

            for (int i = 0; i < numberOfCharacters; ++i)
            {
                characters.Add(GetNextCharacter(packet));
                if (packet.ReadByte() != 255)
                    throw new MalformedPacketException($"{packet.Family}_{packet.Action} packet missing character separator byte", packet);
            }

            return characters;
        }

        private Character GetNextCharacter(IPacket packet)
        {
            var character = new Character.Builder
            {
                Name = packet.ReadBreakString(),
                ID = packet.ReadInt()
            };

            var stats = new CharacterStats().WithNewStat(CharacterStat.Level, packet.ReadChar());

            var gender = packet.ReadChar();
            var hairStyle = packet.ReadChar();
            var hairColor = packet.ReadChar();
            var race = packet.ReadChar();
            var adminLevel = (AdminLevel)packet.ReadChar();
            var boots = packet.ReadShort();
            var armor = packet.ReadShort();
            var hat = packet.ReadShort();
            var shield = packet.ReadShort();
            var weapon = packet.ReadShort();

            var renderProperties = new CharacterRenderProperties.Builder
            { 
               Gender = gender,
               HairStyle = hairStyle,
               HairColor = hairColor,
               Race = race,
               BootsGraphic = boots,
               ArmorGraphic = armor,
               HatGraphic = hat,
               ShieldGraphic = shield,
               WeaponGraphic = weapon
            };

            character.Stats = stats;
            character.AdminLevel = adminLevel;
            character.RenderProperties = renderProperties.ToImmutable();

            return character.ToImmutable();
        }

    }
}
