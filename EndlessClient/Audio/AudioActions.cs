using AutomaticTypeMapper;
using EOLib.Config;
using EOLib.Domain.Map;

namespace EndlessClient.Audio
{
    [AutoMappedType]
    public class AudioActions : IAudioActions
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMfxPlayer _mfxPlayer;

        public AudioActions(IConfigurationProvider configurationProvider,
                            ICurrentMapProvider currentMapProvider,
                            IMfxPlayer mfxPlayer)
        {
            _configurationProvider = configurationProvider;
            _currentMapProvider = currentMapProvider;
            _mfxPlayer = mfxPlayer;
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
    }

    public interface IAudioActions
    {
        void ToggleBackgroundMusic();
    }
}
