using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Guild
{
    public interface IGuildSessionProvider
    {
        int SessionID { get; }
    }

    public interface IGuildSessionRepository
    {
        int SessionID { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GuildSessionRepository : IGuildSessionRepository, IGuildSessionProvider
    {
        public int SessionID { get; set; }

        public GuildSessionRepository()
        {
            SessionID = 0;
        }
    }
}
