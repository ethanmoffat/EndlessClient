using Amadevus.RecordGenerator;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Login
{
    [Record]
    public sealed partial class LoginRequestCompletedData : ITranslatedData
    {
        public IReadOnlyList<string> News { get; }

        public int CharacterWeight { get; }

        public int CharacterMaxWeight { get; }

        public IReadOnlyList<InventoryItem> CharacterItemInventory { get; }

        public IReadOnlyList<InventorySpell> CharacterSpellInventory { get; }

        public IReadOnlyList<Character.Character> MapCharacters { get; }

        public IReadOnlyList<NPC.NPC> MapNPCs { get; }

        public IReadOnlyList<MapItem> MapItems { get; }

        public CharacterLoginReply Error { get; }
    }
}