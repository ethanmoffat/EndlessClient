using Amadevus.RecordGenerator;
using EOLib.Domain.Character;
using EOLib.IO;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Login
{
    [Record]
    public sealed partial class LoginRequestGrantedData : ITranslatedData
    {
        public int SessionID { get; }
        public int CharacterID { get; }

        public int MapID { get; }
        public IReadOnlyList<byte> MapRID { get; }
        public int MapLen { get; }

        public int EifRid { get; }
        public int EifLen { get; }
        public int EnfRid { get; }
        public int EnfLen { get; }
        public int EsfRid { get; }
        public int EsfLen { get; }
        public int EcfRid { get; }
        public int EcfLen { get; }

        public string Name { get; }
        public string Title { get; }
        public string GuildName { get; }
        public string GuildRank { get; }
        public int ClassID { get; }
        public string GuildTag { get; }
        public AdminLevel AdminLevel { get; }

        public CharacterStats CharacterStats { get; }

        public IReadOnlyDictionary<EquipLocation, int> Paperdoll { get; }

        public int GuildRankNum { get; }
        public int JailMap { get; }
        public bool FirstTimePlayer { get; }
    }
}
