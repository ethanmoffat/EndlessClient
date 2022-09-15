using EndlessClient.Audio;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Effects;
using EOLib.Domain.Map;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Map
{
    public class MapGridEffectTarget : IMapGridEffectTarget
    {
        private IEffectRenderer _renderer;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly ICharacterTextures _characterTextures;
        private readonly MapCoordinate _location;

        public Rectangle EffectTargetArea { get; private set; }

        public MapGridEffectTarget(INativeGraphicsManager nativeGraphicsManager,
                                   ISfxPlayer sfxPlayer,
                                   IRenderOffsetCalculator renderOffsetCalculator,
                                   ICharacterRendererProvider characterRendererProvider,
                                   ICharacterTextures characterTextures,
                                   MapCoordinate location)
        {
            _renderer = new EffectRenderer(nativeGraphicsManager, sfxPlayer, this);
            _renderOffsetCalculator = renderOffsetCalculator;
            _characterRendererProvider = characterRendererProvider;
            _characterTextures = characterTextures;
            _location = location;
        }

        public bool EffectIsPlaying() => _renderer.State == EffectState.Playing;

        public void ShowPotionAnimation(int potionId) { }

        public void ShowSpellAnimation(int spellGraphic)
        {
            _renderer.PlayEffect(EffectType.Spell, spellGraphic);
        }

        public void ShowWarpArrive() { }

        public void ShowWarpLeave() { }

        public void ShowWaterSplashies() { }

        public void Update()
        {
            EffectTargetArea = _characterRendererProvider.MainCharacterRenderer
                .Match(
                some: mainRenderer =>
                {
                    var offsetX = _renderOffsetCalculator.CalculateOffsetX(_location);
                    var offsetY = _renderOffsetCalculator.CalculateOffsetY(_location);

                    var mainOffsetX = _renderOffsetCalculator.CalculateOffsetX(mainRenderer.Character.RenderProperties);
                    var mainOffsetY = _renderOffsetCalculator.CalculateOffsetY(mainRenderer.Character.RenderProperties);

                    return new Rectangle(
                        offsetX + 320 - mainOffsetX,
                        offsetY + 168 - mainOffsetY,
                        _characterTextures.Skin.SourceRectangle.Width,
                        _characterTextures.Skin.SourceRectangle.Height);
                },
                none: () => new Rectangle(0, 0, 1, 1));

            _renderer.Update();
        }

        public void Draw(SpriteBatch sb, bool beginHasBeenCalled = true)
        {
            _renderer.DrawBehindTarget(sb, beginHasBeenCalled);
            _renderer.DrawInFrontOfTarget(sb, beginHasBeenCalled);
        }
    }

    public interface IMapGridEffectTarget : IEffectTarget
    {
        void Update();

        void Draw(SpriteBatch sb, bool beginHasBeenCalled = true);
    }
}
