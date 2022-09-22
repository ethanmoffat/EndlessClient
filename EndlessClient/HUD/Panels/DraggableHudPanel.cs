using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class DraggableHudPanel : XNAPanel, IHudPanel
    {
        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (PreviousMouseState.LeftButton == ButtonState.Pressed &&
                CurrentMouseState.LeftButton == ButtonState.Pressed &&
                DrawAreaWithParentOffset.Contains(CurrentMouseState.X, CurrentMouseState.Y) && ShouldClickDrag)
            {
                DrawPosition = new Vector2(
                    DrawPositionWithParentOffset.X + (CurrentMouseState.X - PreviousMouseState.X),
                    DrawPositionWithParentOffset.Y + (CurrentMouseState.Y - PreviousMouseState.Y));
            }

            base.OnUpdateControl(gameTime);
        }
    }
}
