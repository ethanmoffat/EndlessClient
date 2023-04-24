using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface ISoundNotifier
    {
        void NotifySoundEffect(int soundEffectId);
    }

    [AutoMappedType]
    public class NoOpSoundNotifier : ISoundNotifier
    {
        public void NotifySoundEffect(int soundEffectId) { }
    }
}
