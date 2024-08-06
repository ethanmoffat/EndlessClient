using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Priest
{
    public interface IPriestSessionProvider
    {
        int SessionID { get; }
    }

    public interface IPriestSessionRepository
    {
        int SessionID { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class PriestSessionRepository : IPriestSessionProvider, IPriestSessionRepository
    {
        public int SessionID { get; set; }

        public PriestSessionRepository()
        {
            SessionID = 0;
        }
    }
}
