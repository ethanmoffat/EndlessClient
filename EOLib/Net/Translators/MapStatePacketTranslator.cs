// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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

        protected IEnumerable<ICharacter> GetCharacters(IPacket packet)
        {
            var numCharacters = packet.ReadChar();
            if (packet.ReadByte() != 255)
                throw new MalformedPacketException("Missing 255 byte after number of characters", packet);

            for (int i = 0; i < numCharacters; ++i)
            {
                var character = _characterFromPacketFactory.CreateCharacter(packet);

                if (packet.ReadByte() != 255)
                    throw new MalformedPacketException("Missing 255 byte after character", packet);

                yield return character;
            }
        }

        protected IEnumerable<INPC> GetNPCs(IPacket packet)
        {
            while (packet.PeekByte() != 255)
            {
                var index = packet.ReadChar();
                var id = packet.ReadShort();
                var x = packet.ReadChar();
                var y = packet.ReadChar();
                var direction = (EODirection) packet.ReadChar();
                
                yield return new NPC(id, index)
                    .WithX(x)
                    .WithY(y)
                    .WithDirection(direction);
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
