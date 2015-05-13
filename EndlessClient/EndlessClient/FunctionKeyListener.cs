using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient
{
	public class FunctionKeyListener : GameComponent
	{
		private KeyboardState m_prevState;
		//private EOCharacterRenderer m_mainRend;

		public FunctionKeyListener() : base(EOGame.Instance)
		{
			m_prevState = Keyboard.GetState();
			//m_mainRend = World.Instance.ActiveCharacterRenderer;
		}

		public override void Update(GameTime gameTime)
		{
			KeyboardState currState = Keyboard.GetState();

			if (currState.IsKeyUp(Keys.F12) && m_prevState.IsKeyDown(Keys.F12))
			{
				_handleF12();
			}

			m_prevState = currState;
			base.Update(gameTime);
		}

		private void _handleF12()
		{
			if (!((EOGame) Game).API.RequestRefresh())
				((EOGame) Game).LostConnectionDialog();
		}
	}
}
