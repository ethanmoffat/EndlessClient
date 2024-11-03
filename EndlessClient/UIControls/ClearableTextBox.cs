using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class ClearableTextBox : XNATextBox
    {
        public ClearableTextBox(Rectangle area,
                                string spriteFontContentName,
                                Texture2D backgroundTexture = null,
                                Texture2D leftSideTexture = null,
                                Texture2D rightSideTexture = null,
                                Texture2D caretTexture = null)
            : base(area, spriteFontContentName, backgroundTexture, leftSideTexture, rightSideTexture, caretTexture)
        {
        }

        protected override bool HandleKeyPressed(IXNAControl control, KeyboardEventArgs eventArgs)
        {
            if (control != this)
                return false;

            if (eventArgs.Key == Keys.Escape && Selected)
            {
                Text = string.Empty;
                return true;
            }

            return base.HandleKeyPressed(control, eventArgs);
        }
    }
}
