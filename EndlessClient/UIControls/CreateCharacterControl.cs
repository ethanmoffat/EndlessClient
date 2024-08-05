using System;
using EndlessClient.Rendering.Factories;
using EOLib;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class CreateCharacterControl : CharacterControl
    {
        private Vector2 _lastPosition;


        public event EventHandler Clicked;

        // default properties
        public CreateCharacterControl(ICharacterRendererFactory characterRendererFactory)
            : this(GetDefaultProperties(), characterRendererFactory) { }

        // custom render properties
        public CreateCharacterControl(CharacterRenderProperties renderProperties, ICharacterRendererFactory characterRendererFactory)
            : base(Character.Default.WithRenderProperties(renderProperties.WithDirection(EODirection.Down)), characterRendererFactory)
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

            base.OnUpdateControl(gameTime);

            _lastPosition = actualDrawPosition;
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            var nextDirectionInt = (int)RenderProperties.Direction + 1;
            var nextDirection = (EODirection)(nextDirectionInt % 4);
            RenderProperties = RenderProperties.WithDirection(nextDirection);

            Clicked?.Invoke(this, EventArgs.Empty);

            return true;
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
