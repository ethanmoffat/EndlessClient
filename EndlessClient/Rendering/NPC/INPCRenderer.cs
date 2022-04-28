using System;
using EndlessClient.Rendering.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.NPC
{
    public interface INPCRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable, IMapActor, IEffectTarget
    {
        EOLib.Domain.NPC.NPC NPC { get; set; }

        bool IsDead { get; }

        void DrawToSpriteBatch(SpriteBatch spriteBatch);

        void StartDying();
    }
}