// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
	public interface ICharacterRenderer : IDrawable, IGameComponent, IDisposable
	{
		int TopPixel { get; }

		EOGame Game { get; }

		ICharacterRenderProperties RenderProperties { get; set; }

		Rectangle DrawArea { get; }

		void DrawToSpriteBatch(SpriteBatch spriteBatch);
	}
}
