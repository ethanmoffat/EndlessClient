using System;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient
{
	public class InputManager
	{
		private KeyboardState oldKeyboardState, currentKeyboardState;

		public KeyboardState PrevKeyboardState { get { return oldKeyboardState; } }
		public KeyboardState CurrKeyboardState { get { return currentKeyboardState; } }

		public InputManager()
		{
			oldKeyboardState = Keyboard.GetState();
			currentKeyboardState = Keyboard.GetState();
		}

		private void _update()
		{
			oldKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState();
		}

		public void Flush()
		{
			oldKeyboardState = Keyboard.GetState();
			currentKeyboardState = Keyboard.GetState();
		}

		public bool KeyPressed(Keys key)
		{
			_update();
			return oldKeyboardState.IsKeyUp(key) && currentKeyboardState.IsKeyDown(key);
		}

		public bool KeyReleased(Keys key)
		{
			_update();
			return oldKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyUp(key);
		}

		public bool KeyUp(Keys key)
		{
			_update();
			return currentKeyboardState.IsKeyUp(key);
		}

		public bool KeyDown(Keys key)
		{
			_update();
			return currentKeyboardState.IsKeyDown(key);
		}

		public void UpdateTextField(ref string textString)
		{
			oldKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState();

			Keys[] pressedKeys;
			pressedKeys = currentKeyboardState.GetPressedKeys();

			foreach (Keys key in pressedKeys)
			{
				if (oldKeyboardState.IsKeyUp(key))
				{
					if (key == Keys.Back) // overflows
						textString = textString.Remove(textString.Length - 1, 1);
					else
						if (key == Keys.Space)
							textString = textString.Insert(textString.Length, " ");
						else
							textString += key.ToString();
				}
			}
		}
	}
}