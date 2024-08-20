using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Guild
{
    public interface IGuildSessionProvider
    {
        int SessionID { get; }

        string GuildDescription { get; }
    }

    public interface IGuildSessionRepository
    {
        int SessionID { get; set; }

        string GuildDescription { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class GuildSessionRepository : IGuildSessionRepository, IGuildSessionProvider
    {
        public int SessionID { get; set; }
        public string GuildDescription { get; set; }

        public GuildSessionRepository()
        {
            SessionID = 0;
            GuildDescription = "";
        }
    }
}
