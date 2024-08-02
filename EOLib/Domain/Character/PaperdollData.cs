using System.Collections.Generic;
using Amadevus.RecordGenerator;
using EOLib.IO;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Character
{
    [Record]
    public sealed partial class PaperdollData
    {
        public string Name { get; }

        public string Home { get; }

        public string Partner { get; }

        public string Title { get; }

        public string Guild { get; }

        public string Rank { get; }

        public int PlayerID { get; }

        public int Class { get; }

        public int Gender { get; }

        public AdminLevel AdminLevel { get; }

        public IReadOnlyDictionary<EquipLocation, int> Paperdoll { get; }

        public IReadOnlyList<string> QuestNames { get; }

        public CharacterIcon Icon { get; }

        public PaperdollData()
        {
            Paperdoll = new Dictionary<EquipLocation, int>();
            QuestNames = new List<string>();
        }
    }
}