using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Login
{
    public interface ILoginRequestCompletedData : ITranslatedData
    {
        IReadOnlyList<string> News { get; }

        byte CharacterWeight { get; }

        byte CharacterMaxWeight { get; }

        IReadOnlyList<InventoryItem> CharacterItemInventory { get; }

        IReadOnlyList<InventorySpell> CharacterSpellInventory { get; }

        IReadOnlyList<Character.Character> MapCharacters { get; }

        IReadOnlyList<NPC.NPC> MapNPCs { get; }

        IReadOnlyList<IItem> MapItems { get; }

        CharacterLoginReply Error { get; }

        ILoginRequestCompletedData WithNews(IEnumerable<string> newsStrings);

        ILoginRequestCompletedData WithWeight(byte weight);

        ILoginRequestCompletedData WithMaxWeight(byte maxWeight);

        ILoginRequestCompletedData WithInventory(IEnumerable<InventoryItem> inventoryItems);

        ILoginRequestCompletedData WithSpells(IEnumerable<InventorySpell> inventorySpells);

        ILoginRequestCompletedData WithCharacters(IEnumerable<Character.Character> characters);

        ILoginRequestCompletedData WithNPCs(IEnumerable<NPC.NPC> npcs);

        ILoginRequestCompletedData WithItems(IEnumerable<IItem> items);

        ILoginRequestCompletedData WithError(CharacterLoginReply error);
    }
}
