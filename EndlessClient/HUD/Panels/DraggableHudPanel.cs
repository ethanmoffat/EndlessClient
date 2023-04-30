using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class DraggableHudPanel : XNAPanel, IHudPanel
    {
        public event Action DragCompleted;

        private static Option<DraggableHudPanel> _dragging;

        public bool IsBeingDragged => _dragging.HasValue;

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
