using EndlessClient.Audio;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using DomainCharacter = EOLib.Domain.Character.Character;
using DomainNPC = EOLib.Domain.NPC.NPC;

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
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;
        private readonly ICharacterProvider _characterProvider;

        private Option<MapCoordinate> _targetCoordinate;
        private Option<IMapActor> _targetActor;

        private EffectMetadata _metadata;
        private IList<IEffectSpriteInfo> _effectInfo;
        private Stopwatch _lastFrameTimer;

        private int _nextEffectID;
        private Option<MapCoordinate> _nextTargetCoordinate;

        public int EffectID { get; private set; }

        public EffectState State { get; private set; }

        public EffectRenderer(IEffectSpriteManager effectSpriteManager,
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
            _lastFrameTimer = new Stopwatch();
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

        public void QueueEffect(int effectID, MapCoordinate target)
        {
            _nextEffectID = effectID;
            _nextTargetCoordinate = Option.Some(target);
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
            if (!_effectInfo.Any())
                return;

            if (_lastFrameTimer.ElapsedMilliseconds >= 120)
            {
                _lastFrameTimer.Restart();
                _effectInfo.ToList().ForEach(ei => ei.NextFrame());

                var doneEffects = _effectInfo.Where(ei => ei.Done);
                doneEffects.ToList().ForEach(ei => _effectInfo.Remove(ei));
            }

            if (!_effectInfo.Any())
            {
                State = EffectState.Stopped;
                _lastFrameTimer.Stop();

                _nextTargetCoordinate.MatchSome(_ =>
                {
                    EffectID = _nextEffectID;
                    _nextEffectID = 0;

                    _targetCoordinate = _nextTargetCoordinate;
                    _nextTargetCoordinate = Option.None<MapCoordinate>();
                    StartPlaying();
                });
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

        private void StartPlaying()
        {
            _lastFrameTimer.Restart();

            _metadata = _effectSpriteManager.GetEffectMetadata(EffectID);
            _effectInfo = _effectSpriteManager.GetEffectInfo(EffectID, _metadata);

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

            var targetBasePosition = _targetCoordinate.Match(
                some: c => _gridDrawCoordinateCalculator.CalculateBaseLayerDrawCoordinatesFromGridUnits(c),
                none: () => _targetActor.Match(
                    some: actor =>
                    {
                        return actor.SpellTarget switch
                        {
                            DomainCharacter c => GetCharacterBasePosition(c),
                            DomainNPC n => GetNPCBasePosition(n),
                            _ => Vector2.Zero
                        };
                    },
                    none: () => Vector2.Zero));

            foreach (var effectInfo in effectSprites)
            {
                effectInfo.DrawToSpriteBatch(sb, targetBasePosition);
            }

            if (!beginHasBeenCalled)
                sb.End();
        }

        private Vector2 GetCharacterBasePosition(DomainCharacter c)
        {
            var walkExtra = new Vector2(_renderOffsetCalculator.CalculateWalkAdjustX(c.RenderProperties), _renderOffsetCalculator.CalculateWalkAdjustY(c.RenderProperties));
            return _gridDrawCoordinateCalculator.CalculateBaseLayerDrawCoordinatesFromGridUnits(c.RenderProperties.Coordinates()) + walkExtra;
        }

        private Vector2 GetNPCBasePosition(DomainNPC n)
        {
            var walkExtra = new Vector2(_renderOffsetCalculator.CalculateWalkAdjustX(n), _renderOffsetCalculator.CalculateWalkAdjustY(n));
            return _gridDrawCoordinateCalculator.CalculateBaseLayerDrawCoordinatesFromGridUnits(n.X, n.Y) + walkExtra;
        }
    }

    public interface IEffectRenderer
    {
        int EffectID { get; }

        EffectState State { get; }

        void PlayEffect(int effectID, MapCoordinate target);

        void PlayEffect(int effectID, IMapActor target);

        void QueueEffect(int effectID, MapCoordinate target);

        void Restart();

        void Update();

        void DrawBehindTarget(SpriteBatch sb, bool beginHasBeenCalled = true);

        void DrawInFrontOfTarget(SpriteBatch sb, bool beginHasBeenCalled = true);
    }
}
