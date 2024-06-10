using System;
using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Guild
{
    public interface IGuildSessionProvider
    {
        int SessionID { get; }
        IReadOnlyDictionary<string, (int Rank, string RankName)> Members { get; }
    }

    public interface IGuildSessionRepository
    {
        int SessionID { get; set; }
        Dictionary<string, (int Rank, string RankName)> Members { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GuildSessionRepository : IGuildSessionRepository, IGuildSessionProvider
    {
        public int SessionID { get; set; }
        public Dictionary<string, (int Rank, string RankName)> Members { get; set; }

        IReadOnlyDictionary<string, (int Rank, string RankName)> IGuildSessionProvider.Members => Members;

        public GuildSessionRepository()
        {
            SessionID = 0;
            Members = new Dictionary<string, (int Rank, string RankName)>();
        }
    }
}
