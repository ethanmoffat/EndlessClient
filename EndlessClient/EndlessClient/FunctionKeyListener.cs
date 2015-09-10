using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient
{
	public class FunctionKeyListener : InputKeyListenerBase
	{
		public FunctionKeyListener()
		{
			if (Game.Components.Any(x => x is FunctionKeyListener))
				throw new InvalidOperationException("The game already contains an arrow key listener");
			Game.Components.Add(this);
		}

		public override void Update(GameTime gameTime)
		{
			KeyboardState currState = Keyboard.GetState();
			if (!IgnoreInput && Character.State == CharacterActionState.Standing && 
				Character.SelectedSpell <= 0 && !Character.NeedsSpellTarget)
			{
				UpdateInputTime();

				//F1-F8 should be handled the same way: invoke the spell
				for (int key = (int) Keys.F1; key <= (int) Keys.F8; ++key)
				{
					if (IsKeyPressed((Keys) key, currState))
					{
						_handleSpellFunc(key - (int) Keys.F1);
						break;
					}
				}
			}

			if (IsKeyPressedOnce(Keys.F12, currState))
				_handleF12();

			base.Update(gameTime);
		}

		private void _handleSpellFunc(int spellIndex)
		{
			World.Instance.ActiveCharacterRenderer.SelectSpell(spellIndex);
			((EOGame) Game).Hud.SetSelectedSpell(spellIndex);
		}

		private void _handleF12()
		{
			if (!((EOGame) Game).API.RequestRefresh())
				((EOGame) Game).DoShowLostConnectionDialogAndReturnToMainMenu();
		}
	}
}
