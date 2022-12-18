using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
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

        public event EventHandler<MouseEventArgs> OnClick;

        public event EventHandler<MouseEventArgs> OnClickDrag
        {
            add => throw new InvalidOperationException("Unable to set ClickDrag event on clickable area");
            remove => throw new InvalidOperationException("Unable to set ClickDrag event on clickable area");
        }

        public ClickableArea(Rectangle area)
        {
            DrawArea = area;
            ClickArea = area;
        }

        protected override void HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            OnClick?.Invoke(control, eventArgs);
        }
    }
}
