using EndlessClient.Rendering.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable, IMapActor, IEffectTarget, ISpellCaster
    {
        EOLib.Domain.Character.Character Character { get; set; }

        new bool Visible { get; set; }

        bool Transparent { get; set; }

        void SetAbsoluteScreenPosition(int xPosition, int yPosition);

        void SetToCenterScreenPosition();

        void DrawToSpriteBatch(SpriteBatch spriteBatch);

        void ShowName();

        void HideName();
    }
}
