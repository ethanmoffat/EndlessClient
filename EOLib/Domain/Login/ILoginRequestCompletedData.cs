// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Domain.NPC;
using EOLib.Net.Translators;

namespace EOLib.Domain.Login
{
    public interface ILoginRequestCompletedData : ITranslatedData
    {
        IReadOnlyList<string> News { get; }

        byte CharacterWeight { get; }

        byte CharacterMaxWeight { get; }

        IReadOnlyList<IInventoryItem> CharacterItemInventory { get; }

        IReadOnlyList<IInventorySpell> CharacterSpellInventory { get; }

        IReadOnlyList<ICharacter> MapCharacters { get; }

        IReadOnlyList<INPC> MapNPCs { get; }

        IReadOnlyList<IMapItem> MapItems { get; }

        ILoginRequestCompletedData WithNews(IEnumerable<string> newsStrings);

        ILoginRequestCompletedData WithWeight(byte weight);

        ILoginRequestCompletedData WithMaxWeight(byte maxWeight);

        ILoginRequestCompletedData WithInventory(IEnumerable<IInventoryItem> inventoryItems);

        ILoginRequestCompletedData WithSpells(IEnumerable<IInventorySpell> inventorySpells);

        ILoginRequestCompletedData WithCharacters(IEnumerable<ICharacter> characters);

        ILoginRequestCompletedData WithNPCs(IEnumerable<INPC> npcs);

        ILoginRequestCompletedData WithItems(IEnumerable<IMapItem> items);
    }
}
