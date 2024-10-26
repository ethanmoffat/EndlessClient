using System.Collections.Generic;
using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;

namespace EOLib.Domain.Interact.Guild
{
    public interface IGuildSessionProvider
    {
        int SessionID { get; }

        Option<GuildCreationSession> CreationSession { get; }

        string GuildDescription { get; }

        int GuildBalance { get; }

        Option<GuildInfo> GuildInfo { get; }

        IReadOnlyList<GuildMember> GuildMembers { get; }

        string RemoveCandidate { get; }
    }

    public interface IGuildSessionRepository : IResettable
    {
        int SessionID { get; set; }

        Option<GuildCreationSession> CreationSession { get; set; }

        string GuildDescription { get; set; }

        int GuildBalance { get; set; }

        Option<GuildInfo> GuildInfo { get; set; }

        List<GuildMember> GuildMembers { get; set; }

        string RemoveCandidate { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GuildSessionRepository : IGuildSessionRepository, IGuildSessionProvider
    {
        public int SessionID { get; set; }

        public Option<GuildCreationSession> CreationSession { get; set; }

        public string GuildDescription { get; set; }

        public int GuildBalance { get; set; }

        public Option<GuildInfo> GuildInfo { get; set; }

        public List<GuildMember> GuildMembers { get; set; }

        public string RemoveCandidate { get; set; }

        IReadOnlyList<GuildMember> IGuildSessionProvider.GuildMembers => GuildMembers;

        public GuildSessionRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            SessionID = 0;
            CreationSession = Option.None<GuildCreationSession>();
            GuildDescription = string.Empty;
            GuildBalance = 0;
            GuildInfo = Option.None<GuildInfo>();
            GuildMembers = new List<GuildMember>();
            RemoveCandidate = string.Empty;
        }
    }
}
