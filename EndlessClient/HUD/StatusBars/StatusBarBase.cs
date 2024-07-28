using EndlessClient.Rendering;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using XNAControls;

namespace EndlessClient.HUD.StatusBars;

public abstract class StatusBarBase : XNAControl
{
    private readonly INativeGraphicsManager _nativeGraphicsManager;
    private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
    private readonly ICharacterProvider _characterProvider;

    protected readonly XNALabel _label;
    protected readonly Texture2D _texture;

    protected CharacterStats Stats => _characterProvider.MainCharacter.Stats;
    protected Rectangle _sourceRectangleArea;

    protected abstract int StatusBarIndex { get; }

    private Option<DateTime> _labelShowTime;

    public event Action StatusBarClicked;
    public event Action StatusBarClosed;

    protected StatusBarBase(INativeGraphicsManager nativeGraphicsManager,
                            IClientWindowSizeProvider clientWindowSizeProvider,
                            ICharacterProvider characterProvider)
    {
        _nativeGraphicsManager = nativeGraphicsManager;
        _clientWindowSizeProvider = clientWindowSizeProvider;
        _characterProvider = characterProvider;

        _texture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 58, true);

        _label = new XNALabel(Constants.FontSize08)
        {
            AutoSize = false,
            BackColor = Color.Transparent,
            DrawPosition = new Vector2(6, 15),
            ForeColor = ColorConstants.LightGrayText,
            Visible = false
        };
        _label.SetParentControl(this);

        _sourceRectangleArea = new Rectangle(0, 0, 110, 14);

        if (_clientWindowSizeProvider.Resizable)
            _clientWindowSizeProvider.GameWindowSizeChanged += (o, e) => ChangeStatusBarPosition();
    }

    protected abstract void UpdateLabelText();
    protected abstract void DrawStatusBar();

    public override void Initialize()
    {
        _label.Initialize();
        base.Initialize();
    }

    protected override void OnUpdateControl(GameTime gameTime)
    {
        _labelShowTime.MatchSome(x =>
        {
            UpdateLabelText();

            if ((DateTime.Now - x).TotalSeconds >= 4)
            {
                _label.Visible = false;
                _labelShowTime = Option.None<DateTime>();

                StatusBarClosed?.Invoke();
            }
        });

        base.OnUpdateControl(gameTime);
    }

    protected override void OnDrawControl(GameTime gameTime)
    {
        DrawStatusBar();

        if (_labelShowTime.HasValue)
        {
            var dest = new Vector2(DrawAreaWithParentOffset.X,
                                   DrawAreaWithParentOffset.Y + _sourceRectangleArea.Height - 3);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_texture, dest, DropDownSourceRectangle, Color.White);
            _spriteBatch.End();
        }

        base.OnDrawControl(gameTime);
    }

    protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
    {
        _label.Visible = !_label.Visible;
        _labelShowTime = _label.SomeWhen(x => x.Visible).Map(_ => DateTime.Now);

        StatusBarClicked?.Invoke();

        return true;
    }

    protected void ChangeStatusBarPosition()
    {
        var xCoord = (_clientWindowSizeProvider.Width / 2) + StatusBarIndex * DrawArea.Width;
        DrawPosition = new Vector2(xCoord, 0);
    }

    /// <summary>
    /// Source rectangle for the drop-down box in the texture sprite sheet (shown when control is clicked)
    /// </summary>
    private static Rectangle DropDownSourceRectangle => new Rectangle(220, 30, 110, 21);
}