using AutomaticTypeMapper;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class BardController : IBardController
    {
        public void PlayInstrumentNote(int noteIndex)
        {
        }
    }

    public interface IBardController
    {
        void PlayInstrumentNote(int noteIndex);
    }
}
