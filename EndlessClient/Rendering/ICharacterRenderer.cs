// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
	public interface ICharacterRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable
	{
		int TopPixel { get; }

		EOGame Game { get; }

		ICharacterRenderProperties RenderProperties { get; set; }

		Rectangle DrawArea { get; }

		/// <summary>
		/// Set the position of this character renderer at an absolute screen pixel coordinate
		/// </summary>
		void SetAbsoluteScreenPosition(int xPosition, int yPosition);

		/// <summary>
		/// Set the position of this character based on grid coordinates in-game
		/// </summary>
		/// <param name="xCoord">The X coordinate of the map grid</param>
		/// <param name="yCoord">The Y coordinate of the map grid</param>
		/// <param name="mainCharacterOffsetX">The main character's OffsetX property</param>
		/// <param name="mainCharacterOffsetY">The main character's OffsetY property</param>
		void SetGridCoordinatePosition(int xCoord, int yCoord, int mainCharacterOffsetX, int mainCharacterOffsetY);

		void DrawToSpriteBatch(SpriteBatch spriteBatch);
	}
}
