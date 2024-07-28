using AutomaticTypeMapper;
using EndlessClient.Audio;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Effects;

[AutoMappedType]
public class EffectRendererFactory : IEffectRendererFactory
{
    private readonly IEffectSpriteManager _effectSpriteManager;
    private readonly ISfxPlayer _sfxPlayer;
    private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
    private readonly IRenderOffsetCalculator _renderOffsetCalculator;
    private readonly ICharacterProvider _characterProvider;

    public EffectRendererFactory(IEffectSpriteManager effectSpriteManager,
                                 ISfxPlayer sfxPlayer,
                                 IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                 IRenderOffsetCalculator renderOffsetCalculator,
                                 ICharacterProvider characterProvider)
    {
        _effectSpriteManager = effectSpriteManager;
        _sfxPlayer = sfxPlayer;
        _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
        _renderOffsetCalculator = renderOffsetCalculator;
        _characterProvider = characterProvider;
    }

    public IEffectRenderer Create()
    {
        return new EffectRenderer(_effectSpriteManager, _sfxPlayer, _gridDrawCoordinateCalculator, _renderOffsetCalculator, _characterProvider);
    }
}

public interface IEffectRendererFactory
{
    IEffectRenderer Create();
}