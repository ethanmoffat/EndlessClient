using EndlessClient.Rendering.Metadata.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;
using System;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Effects;

public class EffectSpriteInfo : IEffectSpriteInfo
{
    private readonly EffectMetadata _metadata;
    private readonly EffectLayer _layer;
    private readonly Texture2D _graphic;
    private readonly Random _random;

    private int _displayFrame = -1;
    private int _actualFrame;
    private int _iterations;

    public bool OnTopOfCharacter => _layer != EffectLayer.Behind;
    public bool Done => _iterations == _metadata.Loops;

    public EffectSpriteInfo(EffectMetadata metadata,
                            EffectLayer layer,
                            Texture2D graphic)
    {
        _metadata = metadata;
        _layer = layer;
        _graphic = graphic;
        _random = new Random();

        _displayFrame = GetDisplayFrame();
    }

    public void NextFrame()
    {
        if (Done) return;

        _displayFrame = GetDisplayFrame();
        _actualFrame++;

        if (_actualFrame >= _metadata.Frames)
        {
            ResetFrame();
            _iterations++;
        }
    }

    public void Restart()
    {
        ResetFrame();
        _iterations = 0;
    }

    public void DrawToSpriteBatch(SpriteBatch sb, Vector2 gridCoordinatePosition)
    {
        var sourceRect = GetFrameSourceRectangle();
        var drawLocation = GetDrawLocation(sourceRect, gridCoordinatePosition);
        var alpha = _layer == EffectLayer.Transparent ? 128 : 255;

        sb.Draw(_graphic, drawLocation, sourceRect, Color.FromNonPremultiplied(255, 255, 255, alpha));
    }

    private Rectangle GetFrameSourceRectangle()
    {
        var frameWidth = _graphic.Width / _metadata.Frames;
        return new Rectangle(_displayFrame * frameWidth, 0, frameWidth, _graphic.Height);
    }

    private Vector2 GetDrawLocation(Rectangle textureSourceRectangle, Vector2 gridCoordinatePosition)
    {
        const int GridWidth = 64;

        var targetX = gridCoordinatePosition.X + (GridWidth - textureSourceRectangle.Width) / 2;
        var targetY = gridCoordinatePosition.Y - (textureSourceRectangle.Height - (36 + (int)Math.Floor((textureSourceRectangle.Height - 100) / 2.0)));

        var slidingMetadata = _metadata.VerticalSlidingMetadata ?? new VerticalSlidingEffectMetadata(0);
        var positionMetadata = _metadata.PositionOffsetMetadata ?? new PositionOffsetEffectMetadata(new List<int>(), new List<int>());

        var additionalX = _metadata.AnimationType switch
        {
            EffectAnimationType.Position => positionMetadata.OffsetXByFrame.Count < _displayFrame ? positionMetadata.OffsetXByFrame[_displayFrame] : 0,
            _ => 0
        };

        var additionalY = _metadata.AnimationType switch
        {
            EffectAnimationType.VerticalSliding => slidingMetadata.FrameOffsetY * _displayFrame,
            EffectAnimationType.Position => positionMetadata.OffsetYByFrame.Count < _displayFrame ? positionMetadata.OffsetYByFrame[_displayFrame] : 0,
            _ => 0
        };

        return new Vector2(targetX + _metadata.OffsetX + additionalX, targetY + _metadata.OffsetY + additionalY);
    }

    private int GetDisplayFrame()
    {
        return _metadata.AnimationType switch
        {
            EffectAnimationType.Flickering =>
                _random.Next(
                    _metadata.RandomFlickeringMetadata?.FirstFrame ?? 0,
                    1 + (_metadata.RandomFlickeringMetadata?.LastFrame ?? _metadata.RandomFlickeringMetadata?.FirstFrame ?? 0)),
            _ => _displayFrame + 1
        };
    }

    private void ResetFrame()
    {
        _actualFrame = 0;

        if (_metadata.AnimationType != EffectAnimationType.Flickering)
            _displayFrame = 0;
    }
}