using EndlessClient.Rendering.Factories;
using EOLib;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class CreateCharacterControl : CharacterControl
    {
        private Vector2 _lastPosition;

        public event EventHandler Clicked;

        public CreateCharacterControl(ICharacterRendererFactory characterRendererFactory)
            : base(Character.Default.WithRenderProperties(GetDefaultProperties()), characterRendererFactory)
        {
            SetSize(99, 123);
            _lastPosition = Vector2.Zero;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (!ShouldUpdate())
                return;

            var actualDrawPosition = new Vector2(DrawPositionWithParentOffset.X + 40,
                                                 DrawPositionWithParentOffset.Y + 36);

            if (_lastPosition != actualDrawPosition)
                _characterRenderer.SetAbsoluteScreenPosition((int)actualDrawPosition.X, (int)actualDrawPosition.Y);

            if (((CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed) ||
                (CurrentMouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)) &&
                DrawAreaWithParentOffset.ContainsPoint(CurrentMouseState.X, CurrentMouseState.Y))
            {
                var nextDirectionInt = (int)RenderProperties.Direction + 1;
                var nextDirection = (EODirection)(nextDirectionInt % 4);
                RenderProperties = RenderProperties.WithDirection(nextDirection);

                Clicked?.Invoke(this, EventArgs.Empty);
            }

            base.OnUpdateControl(gameTime);

            _lastPosition = actualDrawPosition;
        }

        public void NextGender()
        {
            RenderProperties = RenderProperties.WithGender((RenderProperties.Gender + 1) % 2);
        }

        public void NextRace()
        {
            RenderProperties = RenderProperties.WithRace((RenderProperties.Race + 1) % 6);
        }

        public void NextHairStyle()
        {
            RenderProperties = RenderProperties.WithHairStyle((RenderProperties.HairStyle + 1) % 21);
        }

        public void NextHairColor()
        {
            RenderProperties = RenderProperties.WithHairColor((RenderProperties.HairColor + 1) % 10);
        }

        private static CharacterRenderProperties GetDefaultProperties()
        {
            return new CharacterRenderProperties.Builder { HairStyle = 1 }.ToImmutable();
        }
    }
}
