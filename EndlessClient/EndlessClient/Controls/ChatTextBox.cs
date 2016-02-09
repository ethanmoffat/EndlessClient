// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Controls
{
	/// <summary>
	/// Special instance of an XNATextBox that should ignore input from the number pad (which is used for Emotes)
	/// </summary>
	public class ChatTextBox : XNATextBox
	{
		private bool _ignoreNextInput;

		private bool _ignoreAllInput;

		public ChatTextBox(Rectangle area, Texture2D cursorTexture, string spriteFontContentName)
			: base(area, cursorTexture, spriteFontContentName) { }

		public void ToggleTextInputIgnore()
		{
			_ignoreAllInput = !_ignoreAllInput;
		}

		public override void ReceiveTextInput(char inp)
		{
			if (_ignoreAllInput) return;

			if (!_ignoreNextInput)
				base.ReceiveTextInput(inp);
			else
				_ignoreNextInput = false;
		}

		public override void ReceiveTextInput(string inp)
		{
			if (_ignoreAllInput) return;

			if (!_ignoreNextInput)
				base.ReceiveTextInput(inp);
			else
				_ignoreNextInput = false;
		}

		//public override void ReceiveSpecialInput(Keys key)
		//{
		//	if (_ignoreAllInput) return;

		//	//ignore the emote input keys!
		//	if (key >= Keys.NumPad0 && key <= Keys.NumPad9 || key == Keys.Decimal)
		//	{
		//		_ignoreNextInput = true;
		//	}
		//}
	}
}
