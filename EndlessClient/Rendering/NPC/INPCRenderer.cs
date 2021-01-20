using System;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EOLib.Domain.NPC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.NPC
{
    public interface INPCRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable, IHaveChatBubble, IEffectTarget
    {
        int TopPixel { get; }

        Rectangle MapProjectedDrawArea { get; }

        INPC NPC { get; set; }

        bool IsDead { get; }

        void DrawToSpriteBatch(SpriteBatch spriteBatch);

        void StartDying();
    }
}