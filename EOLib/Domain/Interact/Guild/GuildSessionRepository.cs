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

        Option<GuildInfo> GuildInfo { get; }

        IReadOnlyList<GuildMember> GuildMembers { get; }
    }

    public interface IGuildSessionRepository : IResettable
    {
        int SessionID { get; set; }

        Option<GuildCreationSession> CreationSession { get; set; }

        string GuildDescription { get; set; }

        Option<GuildInfo> GuildInfo { get; set; }

        List<GuildMember> GuildMembers { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GuildSessionRepository : IGuildSessionRepository, IGuildSessionProvider
    {
        public int SessionID { get; set; }

        public Option<GuildCreationSession> CreationSession { get; set; }

        public string GuildDescription { get; set; }

        public Option<GuildInfo> GuildInfo { get; set; }

        public List<GuildMember> GuildMembers { get; set;  }

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
            GuildInfo = Option.None<GuildInfo>();
            GuildMembers = new List<GuildMember>();
        }
    }
}
