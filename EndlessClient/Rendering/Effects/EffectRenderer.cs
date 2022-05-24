using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Audio;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    public enum EffectType
    {
        Invalid,
        Potion,
        Spell,
        WarpOriginal,
        WarpDestination,
        WaterSplashies
    }

    public enum EffectState
    {
        Playing,
        Stopped
    }

    public sealed class EffectRenderer : IEffectRenderer
    {
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IEffectTarget _target;
        private readonly EffectSpriteManager _effectSpriteManager;
        private readonly EffectSoundMapper _effectSoundMapper;

        private IList<IEffectSpriteInfo> _effectInfo;
        private DateTime _lastFrameChange;

        private int _effectID;

        public EffectType EffectType { get; private set; }

        public EffectState State { get; private set; }

        public EffectRenderer(INativeGraphicsManager nativeGraphicsManager,
                              ISfxPlayer sfxPlayer,
                              IEffectTarget target)
        {
            _sfxPlayer = sfxPlayer;
            _target = target;

            _effectSpriteManager = new EffectSpriteManager(nativeGraphicsManager);
            _effectSoundMapper = new EffectSoundMapper();

            _lastFrameChange = DateTime.Now;
            _effectInfo = new List<IEffectSpriteInfo>();
        }

        public void PlayEffect(EffectType effectType, int effectID)
        {
            _effectID = effectID;
            EffectType = effectType;

            _lastFrameChange = DateTime.Now;
            _effectInfo = _effectSpriteManager.GetEffectInfo(EffectType, _effectID);

            State = EffectState.Playing;
            PlaySoundsFromBeginning();
        }

        public void Restart()
        {
            foreach (var effect in _effectInfo)
                effect.Restart();

            State = EffectState.Playing;
            PlaySoundsFromBeginning();
        }

        public void Update()
        {
            if (!_effectInfo.Any())
            {
                State = EffectState.Stopped;
                return;
            }

            var nowTime = DateTime.Now;
            if ((nowTime - _lastFrameChange).TotalMilliseconds > 100)
            {
                _lastFrameChange = nowTime;
                _effectInfo.ToList().ForEach(ei => ei.NextFrame());

                var doneEffects = _effectInfo.Where(ei => ei.Done);
                doneEffects.ToList().ForEach(ei => _effectInfo.Remove(ei));
            }
        }

        public void DrawBehindTarget(SpriteBatch sb, bool beginHasBeenCalled = true)
        {
            if (!_effectInfo.Any())
                return;

            DrawEffects(sb, beginHasBeenCalled, _effectInfo.Where(x => !x.OnTopOfCharacter));
        }

        public void DrawInFrontOfTarget(SpriteBatch sb, bool beginHasBeenCalled = true)
        {
            if (!_effectInfo.Any())
                return;

            DrawEffects(sb, beginHasBeenCalled, _effectInfo.Where(x => x.OnTopOfCharacter));
        }

        private void DrawEffects(SpriteBatch sb, bool beginHasBeenCalled, IEnumerable<IEffectSpriteInfo> effectSprites)
        {
            if (!beginHasBeenCalled)
                sb.Begin();

            foreach (var effectInfo in effectSprites)
                effectInfo.DrawToSpriteBatch(sb, _target.EffectTargetArea);

            if (!beginHasBeenCalled)
                sb.End();
        }

        private void PlaySoundsFromBeginning()
        {
            var soundInfo = _effectSoundMapper.GetSoundEffectsForEffect(EffectType, _effectID);
            foreach (var sound in soundInfo)
                _sfxPlayer.PlaySfx(sound);
        }
    }

    public interface IEffectRenderer
    {
        EffectType EffectType { get; }

        EffectState State { get; }

        void PlayEffect(EffectType effectType, int effectID);

        void Restart();

        void Update();

        void DrawBehindTarget(SpriteBatch sb, bool beginHasBeenCalled = true);

        void DrawInFrontOfTarget(SpriteBatch sb, bool beginHasBeenCalled = true);
    }
}
