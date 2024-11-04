using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class ClickableArea : XNAControl, IXNAButton
    {
        public Rectangle ClickArea
        {
            get => DrawArea;
            set => DrawArea = value;
        }

        public int? FlashSpeed
        {
            get => throw new InvalidOperationException("Unable to get flash speed on clickable area");
            set => throw new InvalidOperationException("Unable to get flash speed on clickable area");
        }

        public event EventHandler<MouseEventArgs> OnMouseDown;

        public event EventHandler<MouseEventArgs> OnMouseUp;

        public event EventHandler<MouseEventArgs> OnClick;

        public event EventHandler<MouseEventArgs> OnClickDrag
        {
            add => throw new InvalidOperationException("Unable to set ClickDrag event on clickable area");
            remove => throw new InvalidOperationException("Unable to set ClickDrag event on clickable area");
        }

        public ClickableArea(Rectangle area)
        {
            ClickArea = area;
        }

        protected override bool HandleMouseDown(IXNAControl control, MouseEventArgs eventArgs)
        {
            OnMouseDown?.Invoke(control, eventArgs);
            return true;
        }

        protected override bool HandleMouseUp(IXNAControl control, MouseEventArgs eventArgs)
        {
            OnMouseUp?.Invoke(control, eventArgs);
            return true;
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            OnClick?.Invoke(control, eventArgs);
            return true;
        }
    }
}
