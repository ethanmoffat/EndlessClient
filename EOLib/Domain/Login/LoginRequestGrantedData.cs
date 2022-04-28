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
        public short SessionID { get; }
        public int CharacterID { get; }

        public short MapID { get; }
        public IReadOnlyList<byte> MapRID { get; }
        public int MapLen { get; }

        public int EifRid { get; }
        public short EifLen { get; }
        public int EnfRid { get; }
        public short EnfLen { get; }
        public int EsfRid { get; }
        public short EsfLen { get; }
        public int EcfRid { get; }
        public short EcfLen { get; }

        public string Name { get; }
        public string Title { get; }
        public string GuildName { get; }
        public string GuildRank { get; }
        public byte ClassID { get; }
        public string GuildTag { get; }
        public AdminLevel AdminLevel { get; }

        public CharacterStats CharacterStats { get; }

        public IReadOnlyDictionary<EquipLocation, short> Paperdoll { get; }

        public byte GuildRankNum { get; }
        public short JailMap { get; }
        public bool FirstTimePlayer { get; }
    }
}
