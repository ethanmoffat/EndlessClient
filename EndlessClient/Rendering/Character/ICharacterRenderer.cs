using System;
using EndlessClient.Rendering.Effects;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable, IMapActor, IEffectTarget, ISpellCaster
    {
        ICharacter Character { get; set; }

        new bool Visible { get; set; }

        bool Transparent { get; set; }

        void SetAbsoluteScreenPosition(int xPosition, int yPosition);

        void SetToCenterScreenPosition();

        void DrawToSpriteBatch(SpriteBatch spriteBatch);
    }
}
