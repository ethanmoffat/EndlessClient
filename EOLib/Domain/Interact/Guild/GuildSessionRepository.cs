using System;
using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Guild
{
    public interface IGuildSessionProvider
    {
        int SessionID { get; }
        int MemberCount { get; }
        Dictionary<string, (int Rank, string RankName)> Members { get; }

        event Action MemberListUpdated;
    }

    public interface IGuildSessionRepository
    {
        int SessionID { get; set; }
        int MemberCount { get; set; }
        Dictionary<string, (int Rank, string RankName)> Members { get; set; }

        event Action MemberListUpdated;
        void OnMemberListUpdated();
    }

    [AutoMappedType(IsSingleton = true)]
    public class GuildSessionRepository : IGuildSessionRepository, IGuildSessionProvider
    {
        public int SessionID { get; set; }
        public int MemberCount { get; set; }
        public Dictionary<string, (int Rank, string RankName)> Members { get; set; }

        public event Action MemberListUpdated;

        public GuildSessionRepository()
        {
            SessionID = 0;
            MemberCount = 0;
            Members = new Dictionary<string, (int Rank, string RankName)>();
        }

        public void OnMemberListUpdated()
        {
            MemberListUpdated?.Invoke();
        }
    }
}
