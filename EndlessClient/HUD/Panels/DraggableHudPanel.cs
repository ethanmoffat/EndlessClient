using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class DraggableHudPanel : XNAPanel, IHudPanel
    {
        public event Action Activated;
        public event Action DragCompleted;

        private static Option<DraggableHudPanel> _dragging;

        public bool IsBeingDragged => _dragging.HasValue;

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            Activated?.Invoke();
            return true;
        }

        protected override bool HandleDragStart(IXNAControl control, MouseEventArgs eventArgs)
        {
            Activated?.Invoke();
            return true;
        }

        protected override bool HandleDrag(IXNAControl control, MouseEventArgs eventArgs)
        {
            _dragging
                .FlatMap(p => p.NoneWhen(q => p == this))
                .MatchNone(() =>
                {
                    _dragging = Option.Some(this);
                    DrawPosition = DrawPositionWithParentOffset + eventArgs.DistanceMoved;
                });

            return true;
        }

        protected override bool HandleDragEnd(IXNAControl control, MouseEventArgs eventArgs)
        {
            _dragging = Option.None<DraggableHudPanel>();
            DragCompleted?.Invoke();

            return true;
        }
    }
}
