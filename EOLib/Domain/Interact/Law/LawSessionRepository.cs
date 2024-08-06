using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Law
{
    public interface ILawSessionProvider
    {
        int SessionID { get; }
    }

    public interface ILawSessionRepository
    {
        int SessionID { get; set; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class LawSessionRepository : ILawSessionRepository, ILawSessionProvider
    {
        public int SessionID { get; set; }

        public LawSessionRepository()
        {
            SessionID = 0;
        }
    }
}
