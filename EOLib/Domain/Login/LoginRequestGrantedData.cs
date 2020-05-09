using System.Collections.Generic;
using System.Linq;
using EOLib.Domain.Character;
using EOLib.IO;

namespace EOLib.Domain.Login
{
    public class LoginRequestGrantedData : ILoginRequestGrantedData
    {
        public short PlayerID { get; private set; }
        public int CharacterID { get; private set; }

        public short MapID { get; private set; }
        public IReadOnlyList<byte> MapRID { get; private set; }
        public int MapLen { get; private set; }

        public int EifRid { get; private set; }
        public short EifLen { get; private set; }
        public int EnfRid { get; private set; }
        public short EnfLen { get; private set; }
        public int EsfRid { get; private set; }
        public short EsfLen { get; private set; }
        public int EcfRid { get; private set; }
        public short EcfLen { get; private set; }

        public string Name { get; private set; }
        public string Title { get; private set; }
        public string GuildName { get; private set; }
        public string GuildRank { get; private set; }
        public byte ClassID { get; private set; }
        public string GuildTag { get; private set; }
        public AdminLevel AdminLevel { get; private set; }

        public ICharacterStats CharacterStats { get; private set; }

        private List<short> _paperdoll = new List<short>((int)EquipLocation.PAPERDOLL_MAX);
        public IReadOnlyList<short> Paperdoll => _paperdoll;

        public byte GuildRankNum { get; private set; }
        public short JailMap { get; private set; }
        public bool FirstTimePlayer { get; private set; }

        public ILoginRequestGrantedData WithPlayerID(short playerID)
        {
            var copy = MakeCopy(this);
            copy.PlayerID = playerID;
            return copy;
        }

        public ILoginRequestGrantedData WithCharacterID(int characterID)
        {
            var copy = MakeCopy(this);
            copy.CharacterID = characterID;
            return copy;
        }

        public ILoginRequestGrantedData WithMapID(short mapID)
        {
            var copy = MakeCopy(this);
            copy.MapID = mapID;
            return copy;
        }

        public ILoginRequestGrantedData WithMapRID(IEnumerable<byte> mapRID)
        {
            var copy = MakeCopy(this);
            copy.MapRID = mapRID.ToList();
            return copy;
        }

        public ILoginRequestGrantedData WithMapLen(int mapLen)
        {
            var copy = MakeCopy(this);
            copy.MapLen = mapLen;
            return copy;
        }

        public ILoginRequestGrantedData WithEifRID(int eifRid)
        {
            var copy = MakeCopy(this);
            copy.EifRid = eifRid;
            return copy;
        }

        public ILoginRequestGrantedData WithEifLen(short eifLen)
        {
            var copy = MakeCopy(this);
            copy.EifLen = eifLen;
            return copy;
        }

        public ILoginRequestGrantedData WithEnfRID(int enfRid)
        {
            var copy = MakeCopy(this);
            copy.EnfRid = enfRid;
            return copy;
        }

        public ILoginRequestGrantedData WithEnfLen(short enfLen)
        {
            var copy = MakeCopy(this);
            copy.EnfLen = enfLen;
            return copy;
        }

        public ILoginRequestGrantedData WithEsfRID(int esfRid)
        {
            var copy = MakeCopy(this);
            copy.EsfRid = esfRid;
            return copy;
        }

        public ILoginRequestGrantedData WithEsfLen(short esfLen)
        {
            var copy = MakeCopy(this);
            copy.EsfLen = esfLen;
            return copy;
        }

        public ILoginRequestGrantedData WithEcfRID(int ecfRid)
        {
            var copy = MakeCopy(this);
            copy.EcfRid = ecfRid;
            return copy;
        }

        public ILoginRequestGrantedData WithEcfLen(short ecfLen)
        {
            var copy = MakeCopy(this);
            copy.EcfLen = ecfLen;
            return copy;
        }

        public ILoginRequestGrantedData WithName(string name)
        {
            var copy = MakeCopy(this);
            copy.Name = name;
            return copy;
        }

        public ILoginRequestGrantedData WithTitle(string title)
        {
            var copy = MakeCopy(this);
            copy.Title = title;
            return copy;
        }

        public ILoginRequestGrantedData WithGuildName(string guildName)
        {
            var copy = MakeCopy(this);
            copy.GuildName = guildName;
            return copy;
        }

        public ILoginRequestGrantedData WithGuildRank(string guildRank)
        {
            var copy = MakeCopy(this);
            copy.GuildRank = guildRank;
            return copy;
        }

        public ILoginRequestGrantedData WithClassId(byte classID)
        {
            var copy = MakeCopy(this);
            copy.ClassID = classID;
            return copy;
        }

        public ILoginRequestGrantedData WithGuildTag(string guildTag)
        {
            var copy = MakeCopy(this);
            copy.GuildTag = guildTag;
            return copy;
        }

        public ILoginRequestGrantedData WithAdminLevel(AdminLevel adminLevel)
        {
            var copy = MakeCopy(this);
            copy.AdminLevel = adminLevel;
            return copy;
        }

        public ILoginRequestGrantedData WithCharacterStats(ICharacterStats stats)
        {
            var copy = MakeCopy(this);
            copy.CharacterStats = stats;
            return copy;
        }

        public ILoginRequestGrantedData WithPaperdoll(IEnumerable<short> paperdollItemIDs)
        {
            var copy = MakeCopy(this);
            copy._paperdoll = paperdollItemIDs.ToList();
            return copy;
        }

        public ILoginRequestGrantedData WithGuildRankNum(byte rankNum)
        {
            var copy = MakeCopy(this);
            copy.GuildRankNum = rankNum;
            return copy;
        }

        public ILoginRequestGrantedData WithJailMap(short jailMapID)
        {
            var copy = MakeCopy(this);
            copy.JailMap = jailMapID;
            return copy;
        }

        public ILoginRequestGrantedData WithFirstTimePlayer(bool isFirstTimePlayer)
        {
            var copy = MakeCopy(this);
            copy.FirstTimePlayer = isFirstTimePlayer;
            return copy;
        }

        private static LoginRequestGrantedData MakeCopy(LoginRequestGrantedData source)
        {
            return new LoginRequestGrantedData
            {
                PlayerID = source.PlayerID,
                CharacterID = source.CharacterID,

                MapID = source.MapID,
                MapRID = source.MapRID,
                MapLen = source.MapLen,

                EifLen = source.EifLen,
                EifRid = source.EifRid,
                EnfLen = source.EnfLen,
                EnfRid = source.EnfRid,
                EsfLen = source.EsfLen,
                EsfRid = source.EsfRid,
                EcfLen = source.EcfLen,
                EcfRid = source.EcfRid,

                Name = source.Name,
                Title = source.Title,
                GuildName = source.GuildName,
                GuildRank = source.GuildRank,
                ClassID = source.ClassID,
                GuildTag = source.GuildTag,
                AdminLevel = source.AdminLevel,

                CharacterStats = source.CharacterStats,

                _paperdoll = source._paperdoll,

                JailMap = source.JailMap,
                GuildRankNum = source.GuildRankNum,
                FirstTimePlayer = source.FirstTimePlayer
            };
        }
    }
}
