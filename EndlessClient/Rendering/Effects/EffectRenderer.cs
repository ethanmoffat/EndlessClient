using EndlessClient.Audio;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Graphics;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Rendering.Effects
{
    public enum EffectState
    {
        Stopped,
        Playing,
    }

    public sealed class EffectRenderer : IEffectRenderer
    {
        private readonly IEffectSpriteManager _effectSpriteManager;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;

        private Option<MapCoordinate> _targetCoordinate;
        private Option<IMapActor> _targetActor;

        private EffectMetadata _metadata;
        private IReadOnlyList<IEffectSpriteInfo> _effectInfo;
        private DateTime _lastFrameChange;

        public int EffectID { get; private set; }

        public EffectState State { get; private set; }

        public EffectRenderer(IEffectSpriteManager effectSpriteManager,
                              ISfxPlayer sfxPlayer,
                              IGridDrawCoordinateCalculator gridDrawCoordinateCalculator)
        {
            _effectSpriteManager = effectSpriteManager;
            _sfxPlayer = sfxPlayer;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;

            _lastFrameChange = DateTime.Now;
            _effectInfo = new List<IEffectSpriteInfo>();
        }

        public void PlayEffect(int effectID, MapCoordinate target)
        {
            EffectID = effectID;
            _targetCoordinate = Option.Some(target);
            StartPlaying();
        }

        public void PlayEffect(int effectID, IMapActor target)
        {
            EffectID = effectID;
            _targetActor = Option.Some(target);
            StartPlaying();
        }

        public void Restart()
        {
            if (State != EffectState.Playing)
                return;

            foreach (var effect in _effectInfo)
                effect.Restart();

            State = EffectState.Playing;

            if (_metadata.SoundEffect != SoundEffectID.NONE)
            {
                _sfxPlayer.PlaySfx(_metadata.SoundEffect);
            }
        }

        public void Update()
        {
            if (State == EffectState.Stopped)
                return;

            var nowTime = DateTime.Now;
            if ((nowTime - _lastFrameChange).TotalMilliseconds >= 120)
            {
                _lastFrameChange = nowTime;
                _effectInfo.ToList().ForEach(ei => ei.NextFrame());
            }
        }

        public void DrawBehindTarget(SpriteBatch sb, bool beginHasBeenCalled = true)
        {
            if (State != EffectState.Playing)
                return;

            DrawEffects(sb, beginHasBeenCalled, _effectInfo.Where(x => !x.Done && !x.OnTopOfCharacter));
        }

        public void DrawInFrontOfTarget(SpriteBatch sb, bool beginHasBeenCalled = true)
        {
            if (State != EffectState.Playing)
                return;

            DrawEffects(sb, beginHasBeenCalled, _effectInfo.Where(x => !x.Done && x.OnTopOfCharacter));
        }

        private void StartPlaying()
        {
            _lastFrameChange = DateTime.Now;

            _metadata = _effectSpriteManager.GetEffectMetadata(EffectID);
            _effectInfo = _effectSpriteManager.GetEffectInfo(EffectID, _metadata);

            foreach (var ei in _effectInfo.Where(x => x.Done))
                ei.Restart();

            State = EffectState.Playing;

            if (_metadata.SoundEffect != SoundEffectID.NONE)
            {
                _sfxPlayer.PlaySfx(_metadata.SoundEffect);
            }
        }

        private void DrawEffects(SpriteBatch sb, bool beginHasBeenCalled, IEnumerable<IEffectSpriteInfo> effectSprites)
        {
            if (!beginHasBeenCalled)
                sb.Begin();

            var targetCoordinate = _targetCoordinate.ValueOr(_targetActor.Match(x => x.Coordinate, () => MapCoordinate.Zero));
            var targetBasePosition = _gridDrawCoordinateCalculator.CalculateBaseLayerDrawCoordinatesFromGridUnits(targetCoordinate);

            foreach (var effectInfo in effectSprites)
            {
                effectInfo.DrawToSpriteBatch(sb, targetBasePosition);
            }

            if (!beginHasBeenCalled)
                sb.End();
        }
    }

    public interface IEffectRenderer
    {
        int EffectID { get; }

        EffectState State { get; }

        void PlayEffect(int effectID, MapCoordinate target);

        void PlayEffect(int effectID, IMapActor target);

        void Restart();

        void Update();

        void DrawBehindTarget(SpriteBatch sb, bool beginHasBeenCalled = true);

        void DrawInFrontOfTarget(SpriteBatch sb, bool beginHasBeenCalled = true);
    }
}
