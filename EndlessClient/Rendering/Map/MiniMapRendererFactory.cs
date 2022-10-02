using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.Map
{
    [AutoMappedType]
    public class MiniMapRendererFactory : IMiniMapRendererFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IMapCellStateProvider _mapCellStateProvider;
        private readonly IENFFileProvider _enfFileProvider;

        public MiniMapRendererFactory(INativeGraphicsManager nativeGraphicsManager,
                                      IClientWindowSizeProvider clientWindowSizeProvider,
                                      ICurrentMapProvider currentMapProvider,
                                      ICurrentMapStateProvider currentMapStateProvider,
                                      ICharacterProvider characterProvider,
                                      IMapCellStateProvider mapCellStateProvider,
                                      IENFFileProvider enfFileProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _currentMapProvider = currentMapProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _characterProvider = characterProvider;
            _mapCellStateProvider = mapCellStateProvider;
            _enfFileProvider = enfFileProvider;
        }

        public MiniMapRenderer Create()
        {
            return new MiniMapRenderer(_nativeGraphicsManager,
                                       _clientWindowSizeProvider,
                                       _currentMapProvider,
                                       _currentMapStateProvider,
                                       _characterProvider,
                                       _mapCellStateProvider,
                                       _enfFileProvider);
        }
    }

    public interface IMiniMapRendererFactory
    {
        MiniMapRenderer Create();
    }
}
