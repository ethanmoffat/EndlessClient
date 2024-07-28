using EndlessClient.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects;

public interface IEffectSpriteInfo
{
    bool OnTopOfCharacter { get; }
    bool Done { get; }

    void NextFrame();
    void Restart();
    void DrawToSpriteBatch(SpriteBatch sb, Vector2 gridCoordinatePosition);
}