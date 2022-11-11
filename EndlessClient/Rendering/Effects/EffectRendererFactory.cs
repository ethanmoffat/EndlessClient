using AutomaticTypeMapper;
using EndlessClient.Audio;

namespace EndlessClient.Rendering.Effects
{
    [AutoMappedType]
    public class EffectRendererFactory : IEffectRendererFactory
    {
        private readonly IEffectSpriteManager _effectSpriteManager;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;

        public EffectRendererFactory(IEffectSpriteManager effectSpriteManager,
                                     ISfxPlayer sfxPlayer,
                                     IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                     IRenderOffsetCalculator renderOffsetCalculator)
        {
            _effectSpriteManager = effectSpriteManager;
            _sfxPlayer = sfxPlayer;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
            _renderOffsetCalculator = renderOffsetCalculator;
        }

        public IEffectRenderer Create()
        {
            return new EffectRenderer(_effectSpriteManager, _sfxPlayer, _gridDrawCoordinateCalculator, _renderOffsetCalculator);
        }
    }

    public interface IEffectRendererFactory
    {
        IEffectRenderer Create();
    }
}
