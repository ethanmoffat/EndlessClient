using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    public abstract class FlickeringEffectSpriteInfo : EffectSpriteInfo
    {
        private const int ABS_MIN_TIME = 0, ABS_MAX_TIME = 500;
        private readonly int _minTimeBetweenFlicker, _maxTimeBetweenFlicker;
        private readonly Random _gen;

        private int _msToWait;
        private DateTime _lastDrawTime;

        protected FlickeringEffectSpriteInfo(int minTimeBetweenFlickerMS,
                                             int maxTimeBetweenFlickerMS,
                                             int numberOfFrames,
                                             int numberOfRepeats,
                                             bool onTopOfCharacter,
                                             int alpha,
                                             Texture2D texture)
            : base(numberOfFrames, numberOfRepeats, onTopOfCharacter, alpha, texture)
        {
            _minTimeBetweenFlicker = Math.Max(minTimeBetweenFlickerMS, ABS_MIN_TIME);
            _maxTimeBetweenFlicker = Math.Max(maxTimeBetweenFlickerMS, ABS_MAX_TIME);
            _gen = new Random();

            _msToWait = GetMillisecondsBetweenDraws();
            _lastDrawTime = DateTime.Now;
        }

        public override void DrawToSpriteBatch(SpriteBatch sb, Rectangle targetRectangle)
        {
            //draw only every certain number of milliseconds, for flicker effect
            if ((DateTime.Now - _lastDrawTime).TotalMilliseconds > _msToWait)
            {
                base.DrawToSpriteBatch(sb, targetRectangle);
            }

            //keep drawing for 100ms (so it doesn't immediately disappear)
            if ((DateTime.Now - _lastDrawTime).TotalMilliseconds > _msToWait + 100)
            {
                _lastDrawTime = DateTime.Now;
                _msToWait = GetMillisecondsBetweenDraws();
                FlickerTimeChanged();
            }
        }

        protected abstract void FlickerTimeChanged();

        private int GetMillisecondsBetweenDraws()
        {
            return _gen.Next(_minTimeBetweenFlicker, _maxTimeBetweenFlicker);
        }
    }
}
