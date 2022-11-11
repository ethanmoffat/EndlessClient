using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;

namespace EndlessClient.Audio
{
    [AutoMappedType]
    public class AudioActions : IAudioActions, ISoundNotifier
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMfxPlayer _mfxPlayer;
        private readonly ISfxPlayer _sfxPlayer;

        public AudioActions(IConfigurationProvider configurationProvider,
                            ICurrentMapProvider currentMapProvider,
                            IMfxPlayer mfxPlayer,
                            ISfxPlayer sfxPlayer)
        {
            _configurationProvider = configurationProvider;
            _currentMapProvider = currentMapProvider;
            _mfxPlayer = mfxPlayer;
            _sfxPlayer = sfxPlayer;
        }

        public void ToggleBackgroundMusic()
        {
            if (!_configurationProvider.MusicEnabled)
            {
                _mfxPlayer.StopBackgroundMusic();
                return;
            }

            var music = _currentMapProvider.CurrentMap.Properties.Music;
            var musicControl = _currentMapProvider.CurrentMap.Properties.Control;
            if (music > 0)
                _mfxPlayer.PlayBackgroundMusic(_currentMapProvider.CurrentMap.Properties.Music, musicControl);
            else
                _mfxPlayer.StopBackgroundMusic();
        }

        public void ToggleSound()
        {
            if (!_configurationProvider.SoundEnabled)
            {
                _sfxPlayer.StopLoopingSfx();
                return;
            }

            var noise = _currentMapProvider.CurrentMap.Properties.AmbientNoise;
            if (noise > 0)
                _sfxPlayer.PlayLoopingSfx((SoundEffectID)noise);
            else
                _sfxPlayer.StopLoopingSfx();
        }

        public void NotifySoundEffect(byte soundEffectId)
        {
            _sfxPlayer.PlaySfx((SoundEffectID)soundEffectId);
        }
    }

    public interface IAudioActions
    {
        void ToggleBackgroundMusic();

        void ToggleSound();
    }
}
