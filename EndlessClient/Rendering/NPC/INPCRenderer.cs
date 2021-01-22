using System;
using EndlessClient.Rendering.Effects;
using EOLib.Domain.NPC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.NPC
{
    public interface INPCRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable, IMapActor, IEffectTarget
    {
        INPC NPC { get; set; }

        bool IsDead { get; }

        void DrawToSpriteBatch(SpriteBatch spriteBatch);

        void StartDying();
    }
}