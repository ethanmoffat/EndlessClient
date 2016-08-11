// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable
    {
        int TopPixel { get; }

        ICharacterRenderProperties RenderProperties { get; set; }

        Rectangle DrawArea { get; }

        void SetAbsoluteScreenPosition(int xPosition, int yPosition);

        void SetToCenterScreenPosition();

        void DrawToSpriteBatch(SpriteBatch spriteBatch);
    }
}
