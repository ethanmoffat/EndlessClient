using System.Collections.Generic;
using System.IO;
using EOLib.Domain.Character;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;

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

        protected IEnumerable<ICharacter> GetCharacters(IPacket packet)
        {
            var characters = new List<ICharacter>();

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
                .WithShieldGraphic(packet.ReadShort());

            var weaponGraphic = packet.ReadShort();
            renderProperties = renderProperties.WithWeaponGraphic(weaponGraphic, _eifFileProvider.EIFFile.IsRangedWeapon(weaponGraphic));

            return character
                .WithRenderProperties(renderProperties)
                .WithStats(stats);
        }

    }
}
