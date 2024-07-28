using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.HUD.Controls;

public class HudBackgroundFrame : XNAControl
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;

    private Texture2D _mainFrame;
    private Texture2D _topLeft;
    private Texture2D _sidebar;
    private Texture2D _topBar;
    private Texture2D _filler;

    public HudBackgroundFrame(INativeGraphicsManager nativeGraphicsManager,
                              IGraphicsDeviceProvider graphicsDeviceProvider)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _graphicsDeviceProvider = graphicsDeviceProvider;
    }

    protected override void LoadContent()
    {
        _mainFrame = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 1, true);
        _topLeft = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 21, true);
        _sidebar = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 22, true);
        _topBar = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 23, true);

        _filler = new Texture2D(_graphicsDeviceProvider.GraphicsDevice, 1, 1);
        _filler.SetData(new[] { Color.FromNonPremultiplied(8, 8, 8, 255) });

        base.LoadContent();
    }

    protected override void OnDrawControl(GameTime gameTime)
    {
        _spriteBatch.Begin();

        _spriteBatch.Draw(_filler, new Rectangle(0, 400, 640, 80), Color.White);

        _spriteBatch.Draw(_topBar, new Vector2(49, 7), Color.White);
        _spriteBatch.Draw(_mainFrame, Vector2.Zero, Color.White);
        _spriteBatch.Draw(_topLeft, Vector2.Zero, Color.White);
        _spriteBatch.Draw(_sidebar, new Vector2(7, 53), Color.White);
        _spriteBatch.Draw(_sidebar, new Vector2(629, 53), new Rectangle(3, 0, 1, _sidebar.Height), Color.White);

        //fill in some extra holes with black lines
        _spriteBatch.Draw(_filler, new Rectangle(542, 0, 1, 8), Color.White);
        _spriteBatch.Draw(_filler, new Rectangle(14, 329, 1, 142), Color.White);
        _spriteBatch.Draw(_filler, new Rectangle(98, 479, 445, 1), Color.White);

        _spriteBatch.End();

        base.OnDrawControl(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _filler.Dispose();
        }

        base.Dispose(disposing);
    }
}