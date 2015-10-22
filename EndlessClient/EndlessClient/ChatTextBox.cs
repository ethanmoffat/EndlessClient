// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient
{
	/// <summary>
	/// Special instance of an XNATextBox that should ignore input from the number pad (which is used for Emotes)
	/// </summary>
	public class ChatTextBox : XNATextBox
	{
		private bool m_ignoreNextInput;

		public bool IgnoreAllInput { get; set; }

		public ChatTextBox(Rectangle area, Texture2D cursorTexture, string spriteFontContentName)
			: base(area, cursorTexture, spriteFontContentName) { }

		public override void ReceiveTextInput(char inp)
		{
			if (IgnoreAllInput) return;

			if (!m_ignoreNextInput)
				base.ReceiveTextInput(inp);
			else
				m_ignoreNextInput = false;
		}
		public override void ReceiveTextInput(string inp)
		{
			if (IgnoreAllInput) return;

			if (!m_ignoreNextInput)
				base.ReceiveTextInput(inp);
			else
				m_ignoreNextInput = false;
		}
		public override void ReceiveSpecialInput(Keys key)
		{
			if (IgnoreAllInput) return;

			//ignore the emote input keys!
			if (key >= Keys.NumPad0 && key <= Keys.NumPad9 || key == Keys.Decimal)
			{
				m_ignoreNextInput = true;
			}
		}
	}
}
