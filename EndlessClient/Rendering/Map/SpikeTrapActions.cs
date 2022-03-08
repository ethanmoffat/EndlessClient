using AutomaticTypeMapper;
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

        public SpikeTrapActions(ICurrentMapStateRepository currentMapStateRepository,
                                ICurrentMapProvider currentMapProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _currentMapProvider = currentMapProvider;
        }

        public void ShowSpikeTrap(MapCoordinate coordinate)
        {
            if (!_currentMapStateRepository.VisibleSpikeTraps.Contains(coordinate) &&
                _currentMapProvider.CurrentMap.Tiles[coordinate.Y, coordinate.X] == TileSpec.SpikesTrap)
            {
                _currentMapStateRepository.VisibleSpikeTraps.Add(coordinate);
            }
        }

        public void ShowSpikeTrap(int characterId)
        {
            if (!_currentMapStateRepository.Characters.ContainsKey(characterId))
                return;

            var character = _currentMapStateRepository.Characters[characterId];
            ShowSpikeTrap(new MapCoordinate(
                character.RenderProperties.GetDestinationX(),
                character.RenderProperties.GetDestinationY()));
        }

        public void HideSpikeTrap(MapCoordinate coordinate)
        {
            _currentMapStateRepository.VisibleSpikeTraps.Remove(coordinate);
        }

        public void HideSpikeTrap(int characterId)
        {
            if (!_currentMapStateRepository.Characters.ContainsKey(characterId))
                return;

            var character = _currentMapStateRepository.Characters[characterId];
            HideSpikeTrap(new MapCoordinate(
                character.RenderProperties.MapX,
                character.RenderProperties.MapY));
        }
    }

    public interface ISpikeTrapActions
    {
        void ShowSpikeTrap(MapCoordinate coordinate);

        void ShowSpikeTrap(int characterId);

        void HideSpikeTrap(MapCoordinate coordinate);

        void HideSpikeTrap(int characterId);
    }
}
