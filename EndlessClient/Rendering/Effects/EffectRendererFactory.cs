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

        public EffectRendererFactory(IEffectSpriteManager effectSpriteManager,
                                     ISfxPlayer sfxPlayer,
                                     IGridDrawCoordinateCalculator gridDrawCoordinateCalculator)
        {
            _effectSpriteManager = effectSpriteManager;
            _sfxPlayer = sfxPlayer;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
        }

        public IEffectRenderer Create()
        {
            return new EffectRenderer(_effectSpriteManager, _sfxPlayer, _gridDrawCoordinateCalculator);
        }
    }

    public interface IEffectRendererFactory
    {
        IEffectRenderer Create();
    }
}
