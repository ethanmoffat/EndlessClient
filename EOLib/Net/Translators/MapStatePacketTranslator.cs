using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;

namespace EOLib.Net.Translators
{
    public abstract class MapStatePacketTranslator<T> : IPacketTranslator<T>
        where T : ITranslatedData
    {
        private readonly ICharacterFromPacketFactory _characterFromPacketFactory;

        protected MapStatePacketTranslator(ICharacterFromPacketFactory characterFromPacketFactory)
        {
            _characterFromPacketFactory = characterFromPacketFactory;
        }

        public abstract T TranslatePacket(IPacket packet);

        protected IEnumerable<Character> GetCharacters(IPacket packet)
        {
            var numCharacters = packet.ReadChar();

            for (int i = 0; i < numCharacters; ++i)
            {
                if (packet.ReadByte() != 255)
                    throw new MalformedPacketException("Missing 255 byte character delimiter", packet);

                var character = _characterFromPacketFactory.CreateCharacter(packet);
                yield return character;
            }

            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing final 255 byte after characters loop", packet);
        }

        protected IEnumerable<NPC> GetNPCs(IPacket packet)
        {
            while (packet.PeekByte() != 255)
            {
                var index = packet.ReadChar();
                var id = packet.ReadShort();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var direction = (EODirection) packet.ReadChar();

                yield return new NPC.Builder()
                {
                    ID = id,
                    Index = index,
                    X = x,
                    Y = y,
                    Direction = direction,
                }.ToImmutable();
            }

            packet.ReadByte(); //consume the tail 255 byte that broke loop iteration
        }

        protected IEnumerable<IItem> GetMapItems(IPacket packet)
        {
            while (packet.ReadPosition < packet.Length)
            {
                var uid = packet.ReadShort();
                var itemID = packet.ReadShort();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var amount = packet.ReadThree();

                yield return new Item(uid, itemID, x, y).WithAmount(amount);
            }
        }
    }
}
