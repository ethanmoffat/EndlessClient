using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (PreviousMouseState.LeftButton == ButtonState.Pressed &&
                CurrentMouseState.LeftButton == ButtonState.Pressed &&
                DrawAreaWithParentOffset.Contains(CurrentMouseState.X, CurrentMouseState.Y) && ShouldClickDrag)
            {
                _dragging
                    .FlatMap(p => p.NoneWhen(q => p == this))
                    .MatchNone(() =>
                    {
                        _dragging = Option.Some(this);

                        DrawPosition = new Vector2(
                            DrawPositionWithParentOffset.X + (CurrentMouseState.X - PreviousMouseState.X),
                            DrawPositionWithParentOffset.Y + (CurrentMouseState.Y - PreviousMouseState.Y));
                    });
            }
            else if (PreviousMouseState.LeftButton == ButtonState.Pressed
                && CurrentMouseState.LeftButton == ButtonState.Released &&
                DrawAreaWithParentOffset.Contains(CurrentMouseState.X, CurrentMouseState.Y) && ShouldClickDrag)
            {
                _dragging = Option.None<DraggableHudPanel>();
                DragCompleted?.Invoke();
            }

            base.OnUpdateControl(gameTime);
        }
    }
}
