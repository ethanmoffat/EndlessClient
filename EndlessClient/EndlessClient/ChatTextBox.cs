using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
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

		public ChatTextBox(Rectangle area, Texture2D cursorTexture, string fontFamily, float fontSize)
			: base(area, cursorTexture, fontFamily, fontSize) { }

		public override void ReceiveTextInput(char inp)
		{
			if (!m_ignoreNextInput)
				base.ReceiveTextInput(inp);
			else
				m_ignoreNextInput = false;
		}
		public override void ReceiveTextInput(string inp)
		{
			if (!m_ignoreNextInput)
				base.ReceiveTextInput(inp);
			else
				m_ignoreNextInput = false;
		}
		public override void ReceiveSpecialInput(Keys key)
		{
			//ignore the emote input keys!
			if (key >= Keys.NumPad0 && key <= Keys.NumPad9 || key == Keys.Decimal)
			{
				m_ignoreNextInput = true;
			}
		}
	}
}
