using AutomaticTypeMapper;
using EndlessClient.Rendering.Factories;
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
        private readonly IENFFileProvider _enfFileProvider;
        private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
        private readonly IRenderTargetFactory _renderTargetFactory;

        public MiniMapRendererFactory(INativeGraphicsManager nativeGraphicsManager,
                                      IRenderTargetFactory renderTargetFactory,
                                      IClientWindowSizeProvider clientWindowSizeProvider,
                                      ICurrentMapProvider currentMapProvider,
                                      ICurrentMapStateProvider currentMapStateProvider,
                                      ICharacterProvider characterProvider,
                                      IENFFileProvider enfFileProvider,
                                      IGridDrawCoordinateCalculator gridDrawCoordinateCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _currentMapProvider = currentMapProvider;
            _currentMapStateProvider = currentMapStateProvider;
            _characterProvider = characterProvider;
            _enfFileProvider = enfFileProvider;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
            _renderTargetFactory = renderTargetFactory;
        }

        public MiniMapRenderer Create()
        {
            return new MiniMapRenderer(_nativeGraphicsManager,
                                       _renderTargetFactory,
                                       _clientWindowSizeProvider,
                                       _currentMapProvider,
                                       _currentMapStateProvider,
                                       _characterProvider,
                                       _enfFileProvider,
                                       _gridDrawCoordinateCalculator);
        }
    }

    public interface IMiniMapRendererFactory
    {
        MiniMapRenderer Create();
    }
}
