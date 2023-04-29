using Amadevus.RecordGenerator;
using EOLib.Domain.Online;
using EOLib.IO;
using System.Collections.Generic;

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

        public IReadOnlyDictionary<EquipLocation, int> Paperdoll { get; }

        public OnlineIcon Icon { get; }

        public PaperdollData()
        {
            Paperdoll = new Dictionary<EquipLocation, int>();
        }
    }
}
