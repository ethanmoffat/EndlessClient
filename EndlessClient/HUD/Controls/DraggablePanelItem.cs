using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System;
using XNAControls;

namespace EndlessClient.HUD.Controls
{
    public abstract class DraggablePanelItem<TRecord> : XNAControl
    {
        protected readonly IDraggableItemContainer _parentContainer;

        // true when a single-click drag is in progress and the icon should follow the mouse
        //    otherwise, drag event handles following the mouse
        private bool _followMouse;

        // the top-left of the parent inventory panel when the drag started
        protected Vector2 OldOffset { get; private set; }

        // true when a drag is in progress (either single click or click + drag)
        public bool IsDragging { get; private set; }

        public TRecord Data { get; protected set; }

        public event EventHandler DraggingStarted;
        public event EventHandler<DragCompletedEventArgs<TRecord>> DraggingFinished;

        protected DraggablePanelItem(IDraggableItemContainer parentContainer)
        {
            _parentContainer = parentContainer;
        }

        public void StartDragging(bool isChainedDrag)
        {
            if (!isChainedDrag && !_parentContainer.NoItemsDragging())
                return;

            IsDragging = true;
            _followMouse = isChainedDrag;

            OldOffset = ImmediateParent.DrawPositionWithParentOffset;
            DrawPosition = MouseExtended.GetState().Position.ToVector2();

            SetControlUnparented();
            Game.Components.Add(this);
            DrawOrder = 1000;

            DraggingStarted?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_followMouse)
            {
                DrawPosition = MouseExtended.GetState().Position.ToVector2() - new Vector2(DrawArea.Width / 2, DrawArea.Height / 2);
            }

            base.OnUpdateControl(gameTime);
        }

        protected override bool HandleDragStart(IXNAControl control, MouseEventArgs eventArgs)
        {
            StartDragging(isChainedDrag: false);
            return true;
        }

        protected override bool HandleDrag(IXNAControl control, MouseEventArgs eventArgs)
        {
            DrawPosition = eventArgs.Position.ToVector2() - new Vector2(DrawArea.Width / 2, DrawArea.Height / 2);
            return true;
        }

        protected override bool HandleDragEnd(IXNAControl control, MouseEventArgs eventArgs)
        {
            StopDragging();
            return true;
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (_followMouse)
            {
                StopDragging();
            }
            else
            {
                StartDragging(isChainedDrag: false);
                _followMouse = true;
            }

            return true;
        }

        /// <summary>
        /// Called when dragging is finished after the DraggingFinished event has been invoked
        /// </summary>
        protected virtual void OnDraggingFinished(DragCompletedEventArgs<TRecord> args)
        {
        }

        private void StopDragging()
        {
            var args = new DragCompletedEventArgs<TRecord>(Data);
            DraggingFinished?.Invoke(this, args);

            OnDraggingFinished(args);

            if (!args.ContinueDrag)
            {
                IsDragging = false;
                OldOffset = Vector2.Zero;
                _followMouse = false;

                Game.Components.Remove(this);
                SetParentControl(_parentContainer);
            }
            else
            {
                _followMouse = true;
            }
        }
    }
}
