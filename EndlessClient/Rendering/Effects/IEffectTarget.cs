using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Effects
{
    public interface IEffectTarget
    {
        Rectangle EffectTargetArea { get; }

        bool EffectIsPlaying();

        void ShowWaterSplashies();

        void ShowWarpArrive();

        void ShowWarpLeave();

        void ShowPotionAnimation(int potionId);

        void ShowSpellAnimation(int spellId);
    }
}
