using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient
{
	public class FunctionKeyListener : GameComponent
	{
		private KeyboardState m_prevState;

		public FunctionKeyListener() : base(EOGame.Instance)
		{
			m_prevState = Keyboard.GetState();
		}

		public override void Update(GameTime gameTime)
		{
			KeyboardState currState = Keyboard.GetState();

			//F1-F8 should be handled the same way: invoke the spell
			for (int key = (int) Keys.F1; key <= (int) Keys.F8; ++key)
			{
				if (currState.IsKeyUp((Keys) key) && m_prevState.IsKeyDown((Keys) key))
				{
					_handleSpellFunc(key - (int) Keys.F1);
					break;
				}
			}

			if (currState.IsKeyUp(Keys.F12) && m_prevState.IsKeyDown(Keys.F12))
				_handleF12();

			m_prevState = currState;
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
