using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class ClickableArea : XNAControl, IXNAButton
    {
        public Rectangle ClickArea { get; set; }

        public int? FlashSpeed
        {
            get => throw new InvalidOperationException("Unable to get flash speed on clickable area");
            set => throw new InvalidOperationException("Unable to get flash speed on clickable area");
        }

        public event EventHandler OnClick;

        public event EventHandler OnClickDrag
        {
            add => throw new InvalidOperationException("Unable to set ClickDrag event on clickable area");
            remove => throw new InvalidOperationException("Unable to set ClickDrag event on clickable area");
        }

        public ClickableArea(Rectangle area)
        {
            DrawArea = area;
            ClickArea = area;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (DrawAreaWithParentOffset.Contains(CurrentMouseState.Position) &&
                CurrentMouseState.LeftButton == ButtonState.Released &&
                PreviousMouseState.LeftButton == ButtonState.Pressed)
            {
                OnClick?.Invoke(this, EventArgs.Empty);
            }

            base.OnUpdateControl(gameTime);
        }
    }
}
