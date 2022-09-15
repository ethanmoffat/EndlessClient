using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Effects;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;

namespace EndlessClient.Rendering.Factories
{
    [AutoMappedType]
    public class MapGridEffectTargetFactory : IMapGridEffectTargetFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly ICharacterTextures _characterTextures;

        public MapGridEffectTargetFactory(INativeGraphicsManager nativeGraphicsManager,
                                          ISfxPlayer sfxPlayer,
                                          IRenderOffsetCalculator renderOffsetCalculator,
                                          ICharacterRendererProvider characterRendererProvider,
                                          ICharacterTextures characterTextures)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _sfxPlayer = sfxPlayer;
            _renderOffsetCalculator = renderOffsetCalculator;
            _characterRendererProvider = characterRendererProvider;
            _characterTextures = characterTextures;
        }

        public IMapGridEffectTarget Create(byte x, byte y)
        {
            if (_characterTextures.Skin == null)
                _characterTextures.Refresh(new CharacterRenderProperties.Builder().ToImmutable());

            return new MapGridEffectTarget(
                _nativeGraphicsManager,
                _sfxPlayer,
                _renderOffsetCalculator,
                _characterRendererProvider,
                _characterTextures,
                new MapCoordinate(x, y));
        }
    }

    public interface IMapGridEffectTargetFactory
    {
        IMapGridEffectTarget Create(byte x, byte y);
    }
}
