// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Controls
{
	public class ClickableCharacterControl : CharacterControl
	{
		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate())
				return;

			var currentState = Mouse.GetState();
			if (((currentState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed) ||
				(currentState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)) &&
				DrawAreaWithOffset.ContainsPoint(currentState.X, currentState.Y))
			{
				var nextDirectionInt = (int)RenderProperties.Direction + 1;
				var nextDirection = (EODirection)(nextDirectionInt % 4);
				RenderProperties = RenderProperties.WithDirection(nextDirection);
			}

			base.Update(gameTime);
		}
	}
}
