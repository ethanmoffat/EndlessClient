using AutomaticTypeMapper;
using EndlessClient.Audio;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;

namespace EndlessClient.Rendering.Map
{
    [AutoMappedType]
    public class SpikeTrapActions : ISpikeTrapActions
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public SpikeTrapActions(ICurrentMapStateRepository currentMapStateRepository,
                                ICurrentMapProvider currentMapProvider,
                                ISfxPlayer sfxPlayer)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
            _sfxPlayer = sfxPlayer;
        }

        public void ShowSpikeTrap(MapCoordinate coordinate, bool isMainCharacter = false)
        {
            if (!_currentMapStateRepository.VisibleSpikeTraps.Contains(coordinate) &&
                _currentMapProvider.CurrentMap.Tiles[coordinate.Y, coordinate.X] == TileSpec.SpikesTrap)
            {
                _currentMapStateRepository.VisibleSpikeTraps.Add(coordinate);

                if (isMainCharacter)
                {
                    _sfxPlayer.PlaySfx(SoundEffectID.Spikes);
                }
            }
        }

        public void ShowSpikeTrap(int characterId)
        {
            if (!_currentMapStateRepository.Characters.TryGetValue(characterId, out var character))
                return;

            ShowSpikeTrap(new MapCoordinate(character.X, character.Y));
        }

        public void HideSpikeTrap(MapCoordinate coordinate)
        {
            _currentMapStateRepository.VisibleSpikeTraps.Remove(coordinate);
        }

        public void HideSpikeTrap(int characterId)
        {
            if (!_currentMapStateRepository.Characters.TryGetValue(characterId, out var character))
                return;

            HideSpikeTrap(new MapCoordinate(character.X, character.Y));
        }
    }

    public interface ISpikeTrapActions
    {
        void ShowSpikeTrap(MapCoordinate coordinate, bool isMainCharacter = false);

        void ShowSpikeTrap(int characterId);

        void HideSpikeTrap(MapCoordinate coordinate);

        void HideSpikeTrap(int characterId);
    }
}
