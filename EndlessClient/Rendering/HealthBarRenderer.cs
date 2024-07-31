using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Rendering
{
    public class HealthBarRenderer : IHealthBarRenderer
    {
        private const int DigitWidth = 9;

        private readonly IMapActor _parentReference;

        private readonly Texture2D _sourceTexture;
        private static readonly Point _numberSpritesOffset, _healthBarSpritesOffset;
        private static readonly Rectangle _healthBarBackgroundSource;

        private Action _doneCallback;
        private bool _isMiss;
        private List<Rectangle> _numberSourceRectangles;
        private Rectangle _healthBarSourceRectangle;

        private float _frameOffset;
        private Vector2 _damageCounterPosition, _healthBarPosition;

        public bool Visible { get; private set; }

        static HealthBarRenderer()
        {
            _numberSpritesOffset = new Point(40, 28);
            _healthBarSpritesOffset = new Point(0, 28);
            _healthBarBackgroundSource = new Rectangle(0, 28, 40, 7);
        }

        public HealthBarRenderer(INativeGraphicsManager nativeGraphicsManager,
                                 IMapActor parentReference)
        {
            _parentReference = parentReference;
            _sourceTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);
            _numberSourceRectangles = new List<Rectangle>();
        }

        public void SetDamage(Option<int> value, int percentHealth, Action doneCallback = null)
        {
            Visible = true;
            _isMiss = !value.HasValue;
            _doneCallback = doneCallback;
            _frameOffset = 0;

            _numberSourceRectangles.Clear();

            value.Match(
                some: v => _numberSourceRectangles.AddRange(
                    GetNumberSourceRectangles(v, isHeal: false)
                        .Select(x => new Rectangle(_numberSpritesOffset + x.Location, x.Size))),
                none: () => _numberSourceRectangles.Add(new Rectangle(_numberSpritesOffset + new Point(92, 0), new Point(30, 11))));

            _healthBarSourceRectangle = GetHealthBarSourceRectangle(percentHealth);
            _healthBarSourceRectangle = new Rectangle(_healthBarSpritesOffset + _healthBarSourceRectangle.Location, _healthBarSourceRectangle.Size);
        }

        public void SetHealth(int value, int percentHealth, Action doneCallback = null)
        {
            Visible = true;
            _isMiss = false;
            _doneCallback = doneCallback;
            _frameOffset = 0;

            _numberSourceRectangles.Clear();
            _numberSourceRectangles.AddRange(
                GetNumberSourceRectangles(value, isHeal: true)
                    .Select(x => new Rectangle(_numberSpritesOffset + x.Location, x.Size)));

            _healthBarSourceRectangle = GetHealthBarSourceRectangle(percentHealth);
            _healthBarSourceRectangle = new Rectangle(_healthBarSpritesOffset + _healthBarSourceRectangle.Location, _healthBarSourceRectangle.Size);
        }

        public void Update(GameTime gameTime)
        {
            if (!Visible) return;

            _frameOffset += .1f;
            if (_frameOffset > 4)
            {
                Visible = false;
                _doneCallback?.Invoke();
            }

            _healthBarPosition = new Vector2(
                _parentReference.HorizontalCenter - _healthBarBackgroundSource.Width / 2f,
                _parentReference.NameLabelY);

            if (_isMiss)
            {
                var xPos = _parentReference.HorizontalCenter - (_numberSourceRectangles[0].Width / 2f);
                var yPos = _parentReference.NameLabelY - _frameOffset - _healthBarBackgroundSource.Height * 2f;
                _damageCounterPosition = new Vector2(xPos, yPos);
            }
            else
            {
                var digitCount = _numberSourceRectangles.Count;
                var xPos = _parentReference.HorizontalCenter - (digitCount * DigitWidth / 2f);
                var yPos = _parentReference.NameLabelY - _frameOffset - _healthBarBackgroundSource.Height * 2f;
                _damageCounterPosition = new Vector2(xPos, yPos);
            }
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            var numberNdx = 0;
            foreach (var numberSource in _numberSourceRectangles)
            {
                spriteBatch.Draw(_sourceTexture, _damageCounterPosition + new Vector2(numberNdx * DigitWidth, 0), numberSource, Color.White);
                numberNdx++;
            }

            spriteBatch.Draw(_sourceTexture, _healthBarPosition, _healthBarBackgroundSource, Color.White);
            spriteBatch.Draw(_sourceTexture, _healthBarPosition, _healthBarSourceRectangle, Color.White);
        }

        private static IEnumerable<Rectangle> GetNumberSourceRectangles(int value, bool isHeal)
        {
            var yCoord = isHeal ? 11 : 0;

            var digits = value.ToString();
            for (int i = 0; i < digits.Length; ++i)
            {
                int next = int.Parse($"{digits[i]}");
                yield return new Rectangle(next * DigitWidth, yCoord, 8, 11);
            }
        }

        private static Rectangle GetHealthBarSourceRectangle(int percentHealth)
        {
            // width of health bar is 40
            // percent -> pixels: (percentHealth / 100) * 40
            const double HealthBarFactor = .4;

            if (percentHealth >= 50)
                return new Rectangle(0, 7, (int)Math.Round(percentHealth * HealthBarFactor), 7);
            else if (percentHealth >= 25)
                return new Rectangle(0, 14, (int)Math.Round(percentHealth * HealthBarFactor), 7);
            else
                return new Rectangle(0, 21, (int)Math.Round(percentHealth * HealthBarFactor), 7);
        }
    }

    public interface IHealthBarRenderer
    {
        bool Visible { get; }

        void SetDamage(Option<int> value, int percentHealth, Action doneCallback = null);

        void SetHealth(int value, int percentHealth, Action doneCallback = null);

        void Update(GameTime gameTime);

        void DrawToSpriteBatch(SpriteBatch spriteBatch);
    }
}