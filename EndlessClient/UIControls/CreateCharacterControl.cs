// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Factories;
using EOLib;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls.Old;

namespace EndlessClient.UIControls
{
    public class CreateCharacterControl : CharacterControl
    {
        private Vector2 _lastPosition;

        public CreateCharacterControl(ICharacterRendererFactory characterRendererFactory)
            : base(GetDefaultProperties(), characterRendererFactory)
        {
            _setSize(99, 123);
            _lastPosition = Vector2.Zero;
        }

        public override void Update(GameTime gameTime)
        {
            if (!ShouldUpdate())
                return;

            var actualDrawPosition = new Vector2(DrawAreaWithOffset.X + 34, DrawAreaWithOffset.Y + 25);
            if (_lastPosition != actualDrawPosition)
                _characterRenderer.SetAbsoluteScreenPosition((int)actualDrawPosition.X, (int)actualDrawPosition.Y);

            var currentState = Mouse.GetState();
            if (((currentState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed) ||
                (currentState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)) &&
                DrawAreaWithOffset.ContainsPoint(currentState.X, currentState.Y))
            {
                var nextDirectionInt = (int)RenderProperties.Direction + 1;
                var nextDirection = (EODirection)(nextDirectionInt % 4);
                RenderProperties = RenderProperties.WithDirection(nextDirection);
            }

            base.Update(gameTime);

            _lastPosition = actualDrawPosition;
        }

        public void NextGender()
        {
            RenderProperties = RenderProperties.WithGender((byte)((RenderProperties.Gender + 1) % 2));
        }

        public void NextRace()
        {
            RenderProperties = RenderProperties.WithRace((byte)((RenderProperties.Race + 1) % 6));
        }

        public void NextHairStyle()
        {
            RenderProperties = RenderProperties.WithHairStyle((byte)((RenderProperties.HairStyle + 1) % 21));
        }

        public void NextHairColor()
        {
            RenderProperties = RenderProperties.WithHairColor((byte)((RenderProperties.HairColor + 1) % 10));
        }

        private static ICharacterRenderProperties GetDefaultProperties()
        {
            return new CharacterRenderProperties().WithHairStyle(1);
        }
    }
}
