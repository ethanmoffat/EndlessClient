using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EndlessClient.Rendering.NPC
{
    public interface INPCRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable, IMapActor
    {
        EOLib.Domain.NPC.NPC NPC { get; set; }

        bool IsDead { get; }

        bool IsClickablePixel(Point currentMousePosition);

        void DrawToSpriteBatch(SpriteBatch spriteBatch);

        void StartDying();
    }
}