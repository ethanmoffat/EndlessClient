// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EndlessClient.Rendering.MapEntityRenderers;
using EOLib.Domain.Character;
using EOLib.Domain.Map;

namespace EndlessClient.Rendering.Factories
{
    public class MapRendererFactory : IMapRendererFactory
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IMapEntityRendererProvider _mapEntityRendererProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMapRenderDistanceCalculator _mapRenderDistanceCalculator;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly ICharacterRendererRepository _characterRendererRepository;
        private readonly ICharacterStateCache _characterStateCache;

        public MapRendererFactory(IEndlessGameProvider endlessGameProvider,
            IRenderTargetFactory renderTargetFactory,
            IMapEntityRendererProvider mapEntityRendererProvider,
            ICharacterProvider characterProvider,
            ICurrentMapProvider currentMapProvider,
            IMapRenderDistanceCalculator mapRenderDistanceCalculator,
            ICharacterRendererFactory characterRendererFactory,
            ICharacterRendererRepository characterRendererRepository,
            ICharacterStateCache characterStateCache)
        {
            _endlessGameProvider = endlessGameProvider;
            _renderTargetFactory = renderTargetFactory;
            _mapEntityRendererProvider = mapEntityRendererProvider;
            _characterProvider = characterProvider;
            _currentMapProvider = currentMapProvider;
            _mapRenderDistanceCalculator = mapRenderDistanceCalculator;
            _characterRendererFactory = characterRendererFactory;
            _characterRendererRepository = characterRendererRepository;
            _characterStateCache = characterStateCache;
        }

        public IMapRenderer CreateMapRenderer()
        {
            return new MapRenderer(_endlessGameProvider.Game,
                                   _renderTargetFactory,
                                   _mapEntityRendererProvider,
                                   _characterProvider,
                                   _currentMapProvider,
                                   _mapRenderDistanceCalculator,
                                   _characterRendererFactory,
                                   _characterRendererRepository,
                                   _characterStateCache);
        }
    }
}