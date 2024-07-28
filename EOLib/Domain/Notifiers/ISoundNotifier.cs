using AutomaticTypeMapper;

namespace EOLib.Domain.Notifiers
{
    public interface ISoundNotifier
    {
        void NotifySoundEffect(int soundEffectId);

        void NotifyMusic(int musicEffectId, bool isJukebox);
    }

    [AutoMappedType]
    public class NoOpSoundNotifier : ISoundNotifier
    {
        public void NotifySoundEffect(int soundEffectId) { }

        public void NotifyMusic(int musicEffectId, bool isJukebox) { }
    }
}