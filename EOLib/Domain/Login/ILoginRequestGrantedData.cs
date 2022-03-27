using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Net.Translators;

namespace EOLib.Domain.Login
{
    public interface ILoginRequestGrantedData : ITranslatedData
    {
        short SessionID { get; }
        int CharacterID { get; }

        short MapID { get; }
        IReadOnlyList<byte> MapRID { get; }
        int MapLen { get; }

        int EifRid { get; }
        short EifLen { get; }
        int EnfRid { get; }
        short EnfLen { get; }
        int EsfRid { get; }
        short EsfLen { get; }
        int EcfRid { get; }
        short EcfLen { get; }

        string Name { get; }
        string Title { get; }
        string GuildName { get; }
        string GuildRank { get; }
        byte ClassID { get; }
        string GuildTag { get; }
        AdminLevel AdminLevel { get; }

        ICharacterStats CharacterStats { get; }

        IReadOnlyList<short> Paperdoll { get; }

        byte GuildRankNum { get; }
        short JailMap { get; }
        bool FirstTimePlayer { get; }

        ILoginRequestGrantedData WithSessionID(short sessionID);
        ILoginRequestGrantedData WithCharacterID(int characterID);
        ILoginRequestGrantedData WithMapID(short mapID);
        ILoginRequestGrantedData WithMapRID(IEnumerable<byte> mapRID);
        ILoginRequestGrantedData WithMapLen(int mapLen);
        ILoginRequestGrantedData WithEifRID(int eifRid);
        ILoginRequestGrantedData WithEifLen(short eifLen);
        ILoginRequestGrantedData WithEnfRID(int enfRid);
        ILoginRequestGrantedData WithEnfLen(short enfLen);
        ILoginRequestGrantedData WithEsfRID(int esfRid);
        ILoginRequestGrantedData WithEsfLen(short esfLen);
        ILoginRequestGrantedData WithEcfRID(int ecfRid);
        ILoginRequestGrantedData WithEcfLen(short ecfLen);
        ILoginRequestGrantedData WithName(string name);
        ILoginRequestGrantedData WithTitle(string title);
        ILoginRequestGrantedData WithGuildName(string guildName);
        ILoginRequestGrantedData WithGuildRank(string guildRank);
        ILoginRequestGrantedData WithClassId(byte classID);
        ILoginRequestGrantedData WithGuildTag(string guildTag);
        ILoginRequestGrantedData WithAdminLevel(AdminLevel adminLevel);
        ILoginRequestGrantedData WithCharacterStats(ICharacterStats stats);
        ILoginRequestGrantedData WithPaperdoll(IEnumerable<short> paperdollItemIDs);
        ILoginRequestGrantedData WithGuildRankNum(byte rankNum);
        ILoginRequestGrantedData WithJailMap(short jailMapID);
        ILoginRequestGrantedData WithFirstTimePlayer(bool isFirstTimePlayer);
    }
}
