using System.Collections.Generic;
using System.IO;
using EOLib.Domain.Character;

namespace EOLib.Net.Translators
{
    public abstract class CharacterDisplayPacketTranslator<T> : IPacketTranslator<T>
        where T : ITranslatedData
    {
        public abstract T TranslatePacket(IPacket packet);

        protected IEnumerable<ICharacter> GetCharacters(IPacket packet)
        {
            var characters = new List<ICharacter>();

            var numberOfCharacters = (int)packet.ReadChar();
            packet.Seek(1, SeekOrigin.Current);

            for (int i = 0; i < numberOfCharacters; ++i)
            {
                if (packet.ReadByte() != 255)
                    throw new MalformedPacketException("Login packet missing character separator byte", packet);
                characters.Add(GetNextCharacter(packet));
            }

            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Login packet missing character separator byte", packet);

            return characters;
        }

        private ICharacter GetNextCharacter(IPacket packet)
        {
            ICharacter character = new Character()
                .WithName(packet.ReadBreakString())
                .WithID(packet.ReadInt());

            var stats = new CharacterStats()
                .WithNewStat(CharacterStat.Level, packet.ReadChar());

            var renderProperties = new CharacterRenderProperties()
                .WithGender(packet.ReadChar())
                .WithHairStyle(packet.ReadChar())
                .WithHairColor(packet.ReadChar())
                .WithRace(packet.ReadChar());

            character = character.WithAdminLevel((AdminLevel)packet.ReadChar());

            renderProperties = renderProperties
                .WithBootsGraphic(packet.ReadShort())
                .WithArmorGraphic(packet.ReadShort())
                .WithHatGraphic(packet.ReadShort())
                .WithShieldGraphic(packet.ReadShort())
                .WithWeaponGraphic(packet.ReadShort());

            return character
                .WithRenderProperties(renderProperties)
                .WithStats(stats);
        }

    }
}
