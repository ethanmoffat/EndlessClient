using EndlessClient.HUD;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XNAControls;

namespace EndlessClient.UIControls;

public class StatusBarLabel : XNALabel
{
    private const int STATUS_LABEL_DISPLAY_TIME_MS = 3000;

    private readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
    private readonly IStatusLabelTextProvider _statusLabelTextProvider;

    private readonly Rectangle _leftSide = new Rectangle(58, 451, 12, 20);
    private readonly Rectangle _background = new Rectangle(70, 451, 548, 20);
    private readonly Rectangle _rightSide = new Rectangle(618, 451, 12, 20);

    private readonly Texture2D _hudBackground;

    public StatusBarLabel(INativeGraphicsManager nativeGraphicsManager,
                          IClientWindowSizeProvider clientWindowSizeProvider,
                          IStatusLabelTextProvider statusLabelTextProvider)
        : base(Constants.FontSize07)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _clientWindowSizeProvider = clientWindowSizeProvider;
        _statusLabelTextProvider = statusLabelTextProvider;

        if (_clientWindowSizeProvider.Resizable)
        {
            DrawArea = new Rectangle(40, _clientWindowSizeProvider.Height - 15, 1, 1);
            _clientWindowSizeProvider.GameWindowSizeChanged += (_, _) => DrawArea = new Rectangle(40, _clientWindowSizeProvider.Height - 15, 1, 1);

            _hudBackground = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 1, false, true);
        }
        else
        {
            DrawArea = new Rectangle(97, _clientWindowSizeProvider.Height - 26, 1, 1);
        }
    }

    protected override void OnUnconditionalUpdateControl(GameTime gameTime)
    {
        if (Text != _statusLabelTextProvider.StatusText)
        {
            Text = _statusLabelTextProvider.StatusText;
        }

        if ((DateTime.Now - _statusLabelTextProvider.SetTime).TotalMilliseconds > STATUS_LABEL_DISPLAY_TIME_MS)
        {
            Text = string.Empty;
        }

        base.OnUnconditionalUpdateControl(gameTime);
    }

    protected override void OnDrawControl(GameTime gameTime)
    {
        if (_clientWindowSizeProvider.Resizable)
        {
            var bgDrawArea = new Rectangle(0, _clientWindowSizeProvider.Height - 20, _clientWindowSizeProvider.Width, 20);

            _spriteBatch.Begin(samplerState: SamplerState.LinearWrap);
            _spriteBatch.Draw(_hudBackground, bgDrawArea, _background, Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.Draw(_hudBackground, bgDrawArea.Location.ToVector2(), _leftSide, Color.White);
            _spriteBatch.Draw(_hudBackground, new Vector2(_clientWindowSizeProvider.Width - _rightSide.Width, bgDrawArea.Y), _rightSide, Color.White);
            _spriteBatch.End();
        }

        base.OnDrawControl(gameTime);
    }
}