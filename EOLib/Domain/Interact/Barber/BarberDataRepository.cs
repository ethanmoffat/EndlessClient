using AutomaticTypeMapper;

namespace EOLib.Domain.Interact.Barber
{
    public interface IBarberDataRepository : IResettable
    {
        int SessionID { get; set; }
    }

    public interface IBarberDataProvider : IResettable
    {
        int SessionID { get; }
    }


    [AutoMappedType(IsSingleton = true)]
    public class BarberDataRepository : IBarberDataRepository, IBarberDataProvider
    {
        private int _sessionID;
        public int SessionID
        {
            get => _sessionID;
            set
            {
                _sessionID = value;
            }
        }

        public BarberDataRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            SessionID = 0;
        }
    }

}