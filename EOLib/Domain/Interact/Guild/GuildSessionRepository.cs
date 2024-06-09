using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Guild
{
    public interface IGuildSessionProvider
    {
        int SessionID { get; }
        int MemberCount { get; }
        List<string> Names { get; }
    }

    public interface IGuildSessionRepository
    {
        int SessionID { get; set; }
        int MemberCount { get; set; }
        List<string> Names { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GuildSessionRepository : IGuildSessionRepository, IGuildSessionProvider
    {
        public int SessionID { get; set; }
        public int MemberCount { get; set; }
        public List<string> Names { get; set; }

        public GuildSessionRepository()
        {
            SessionID = 0;
            MemberCount = 0;
            Names = new List<string>();
        }
    }
}
