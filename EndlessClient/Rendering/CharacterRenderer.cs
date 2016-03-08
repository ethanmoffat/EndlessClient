// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Data.BLL;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering
{
	public class CharacterRenderer : DrawableGameComponent, ICharacterRenderer
	{
		public ICharacterRenderProperties RenderProperties { get; set; }

		public Rectangle DrawArea { get; private set; }

		public int TopPixel { get; private set; }

		public CharacterRenderer(EOGame game, ICharacterRenderProperties renderProperties)
			: base(game)
		{
			RenderProperties = renderProperties;
		}

		public override void Initialize()
		{
			//set top pixel
			base.Initialize();
		}

		protected override void LoadContent()
		{
			//load default textures, set top pixel
			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			//get textures based on render properties and draw
			base.Draw(gameTime);
		}
	}
}
