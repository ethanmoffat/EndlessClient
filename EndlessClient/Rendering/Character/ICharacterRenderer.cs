using System;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Effects;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable, IHaveChatBubble, IEffectTarget
    {
        int? TopPixel { get; }

        ICharacter Character { get; set; }

        bool Transparent { get; set; }

        void SetAbsoluteScreenPosition(int xPosition, int yPosition);

        void SetToCenterScreenPosition();

        void DrawToSpriteBatch(SpriteBatch spriteBatch);
    }
}
