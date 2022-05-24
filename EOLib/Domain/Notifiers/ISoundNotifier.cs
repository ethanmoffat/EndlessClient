using AutomaticTypeMapper;
using EOLib.Domain.Map;

namespace EOLib.Domain.Notifiers
{
    public interface ISoundNotifier
    {
        void NotifySoundEffect(byte soundEffectId);
    }

    [AutoMappedType]
    public class NoOpSoundNotifier : ISoundNotifier
    {
        public void NotifySoundEffect(byte soundEffectId) { }
    }
}
