// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Controls
{
	public class CharacterControl : XNAControl
	{
		public ICharacterRenderProperties RenderProperties
		{
			get { return _characterRenderer.RenderProperties; }
			protected set { _characterRenderer.RenderProperties = value; }
		}

		protected readonly ICharacterRenderer _characterRenderer;

		public CharacterControl(ICharacterRenderProperties initialProperties)
		{
			_characterRenderer = new CharacterRenderer((EOGame)Game, initialProperties);
		}

		public override void Initialize()
		{
			_characterRenderer.Initialize();
			_characterRenderer.SetAbsoluteScreenPosition(DrawAreaWithOffset.X, DrawAreaWithOffset.Y);

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate())
				return;
			
			_characterRenderer.Update(gameTime);

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			_characterRenderer.Draw(gameTime);

			base.Draw(gameTime);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_characterRenderer.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
