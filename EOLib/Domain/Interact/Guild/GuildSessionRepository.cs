using AutomaticTypeMapper;
using Optional;

namespace EOLib.Domain.Interact.Guild
{
    public interface IGuildSessionProvider
    {
        int SessionID { get; }
        Option<GuildCreationSession> CreationSession { get; }
    }

    public interface IGuildSessionRepository
    {
        int SessionID { get; set; }
        Option<GuildCreationSession> CreationSession { get; set; }


        [AutoMappedType(IsSingleton = true)]
        public class GuildSessionRepository : IGuildSessionRepository, IGuildSessionProvider
        {
            public int SessionID { get; set; }
            public Option<GuildCreationSession> CreationSession { get; set; }

            public GuildSessionRepository()
            {
                SessionID = 0;
                CreationSession = Option.None<GuildCreationSession>();
            }
        }
    }
}
