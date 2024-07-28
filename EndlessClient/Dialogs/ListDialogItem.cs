using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System;
using XNAControls;

namespace EndlessClient.Dialogs;

public class ListDialogItem : XNAControl
{
    public enum ListItemStyle
    {
        Small,
        SmallKeyValue,
        Large
    }

    private int _index;
    private int _xOffset, _yOffset;

    protected IXNALabel _primaryText;
    protected IXNALabel _subText;

    private readonly Texture2D _gfxPadThing;
    private readonly Texture2D _backgroundColor;

    private readonly BaseEODialog _parentDialog;

    private bool _drawBackground;

    public int Index
    {
        get { return _index; }
        set
        {
            _index = value;
            DrawPosition = new Vector2(DrawPosition.X, OffsetY + (_index * (Style == ListItemStyle.Large ? 36 : 16)));
        }
    }

    public int OffsetX
    {
        get
        {
            return _xOffset;
        }
        set
        {
            int oldOff = _xOffset;
            _xOffset = value;
            DrawPosition += new Vector2(_xOffset - oldOff, 0);
        }
    }

    public int OffsetY
    {
        get
        {
            return _yOffset;
        }
        set
        {
            int oldOff = _yOffset;
            _yOffset = value;
            DrawPosition += new Vector2(0, _yOffset - oldOff);
        }
    }

    public ListItemStyle Style { get; set; }

    public string PrimaryText
    {
        get { return _primaryText.Text; }
        set
        {
            _primaryText.Text = value;
            _primaryText.ResizeBasedOnText();
        }
    }

    public string SubText
    {
        get { return _subText.Text; }
        set
        {
            _subText.Text = value;
            if (Style != ListItemStyle.SmallKeyValue)
                _subText.ResizeBasedOnText();
        }
    }

    public Texture2D IconGraphic { get; set; }

    public Rectangle? IconGraphicSource { get; set; }

    public bool ShowIconBackGround { get; set; }

    public bool ShowSubtext
    {
        get => _subText.Visible;
        set => ((XNAControl)_subText).Visible = value;
    }

    public bool UnderlineLinks { get; set; } = true;

    public object Data { get; set; }

    public event EventHandler<MouseEventArgs> RightClick;
    public event EventHandler<MouseEventArgs> LeftClick;

    public ListDialogItem(BaseEODialog parent, ListItemStyle style, int listIndex = -1)
    {
        _parentDialog = parent;

        DrawPosition += new Vector2(17, 0);

        Style = style;
        if (listIndex >= 0)
            Index = listIndex;

        SetSize(232, Style == ListItemStyle.Large ? 36 : 13);

        int colorFactor = Style == ListItemStyle.Large ? 0xc8 : 0xb4;

        _primaryText = new XNALabel(Constants.FontSize08pt5)
        {
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = Color.FromNonPremultiplied(colorFactor, colorFactor, colorFactor, 0xff),
            DrawPosition = Style == ListItemStyle.Large ? new Vector2(56, 5) : new Vector2(2, 0),
            TextAlign = LabelAlignment.TopLeft,
            Text = " "
        };
        _primaryText.ResizeBasedOnText();
        _primaryText.SetParentControl(this);
        _primaryText.Initialize();

        if (Style == ListItemStyle.SmallKeyValue)
        {
            _subText = new XNALabel(Constants.FontSize08pt5)
            {
                BackColor = _primaryText.BackColor,
                ForeColor = _primaryText.ForeColor,
                DrawArea = new Rectangle(2, 0, 225, 13),
                TextAlign = LabelAlignment.Right,
                Text = " ",
                Visible = true
            };
        }
        else
        {
            _subText = new XNALabel(Constants.FontSize08pt5)
            {
                AutoSize = true,
                BackColor = _primaryText.BackColor,
                ForeColor = _primaryText.ForeColor,
                DrawPosition = Style == ListItemStyle.Large ? new Vector2(56, 20) : new Vector2(100, 0),
                Text = " ",
                Visible = Style == ListItemStyle.Large
            };
            _subText.ResizeBasedOnText();
        }
        _subText.SetParentControl(this);
        _subText.Initialize();

        _gfxPadThing = parent.GraphicsManager.TextureFromResource(GFXTypes.MapTiles, 0, true);
        ShowIconBackGround = Style == ListItemStyle.Large;

        _backgroundColor = new Texture2D(Game.GraphicsDevice, 1, 1);
        _backgroundColor.SetData(new[] { Color.White });

        SetParentControl(parent);

        OnMouseEnter += (_, _) => _drawBackground = true;
        OnMouseLeave += (_, _) => _drawBackground = false;

        OffsetY = Style == ListItemStyle.Large ? 25 : 45;
    }

    public void SetPrimaryClickAction(EventHandler<MouseEventArgs> onClickAction)
    {
        var oldText = _primaryText;
        _primaryText = new XNAHyperLink(Constants.FontSize08pt5)
        {
            AutoSize = false,
            BackColor = oldText.BackColor,
            DrawArea = oldText.DrawArea,
            ForeColor = oldText.ForeColor,
            MouseOverColor = oldText.ForeColor,
            Text = oldText.Text,
            Underline = UnderlineLinks
        };

        ((XNAHyperLink)_primaryText).OnClick += onClickAction;

        _primaryText.SetParentControl(this);
        _primaryText.Initialize();
        _primaryText.ResizeBasedOnText();

        oldText.Dispose();

        if (Style == ListItemStyle.Small || Style == ListItemStyle.SmallKeyValue)
            LeftClick += onClickAction;
    }

    public void SetSubtextClickAction(EventHandler<MouseEventArgs> onClickAction)
    {
        if (Style == ListItemStyle.Small || Style == ListItemStyle.SmallKeyValue)
            throw new InvalidOperationException("Unable to set subtext click action when style is Small");

        var oldText = _subText;
        _subText = new XNAHyperLink(Constants.FontSize08pt5)
        {
            AutoSize = false,
            BackColor = oldText.BackColor,
            DrawArea = oldText.DrawArea,
            ForeColor = oldText.ForeColor,
            MouseOverColor = oldText.ForeColor,
            Text = oldText.Text,
            Underline = UnderlineLinks
        };

        ((XNAHyperLink)_subText).OnClick += onClickAction;

        _subText.SetParentControl(this);
        _subText.Initialize();
        _subText.ResizeBasedOnText();

        oldText.Dispose();
    }

    public void Highlight()
    {
        _primaryText.ForeColor = Color.FromNonPremultiplied(0xf0, 0xf0, 0xf0, 0xff);
    }

    public void ClearHighlight()
    {
        int colorFactor = Style == ListItemStyle.Large ? 0xc8 : 0xb4;
        _primaryText.ForeColor = Color.FromNonPremultiplied(colorFactor, colorFactor, colorFactor, 0xff);
    }

    protected override void OnDrawControl(GameTime gameTime)
    {
        base.OnDrawControl(gameTime);

        _spriteBatch.Begin();
        if (_drawBackground)
        {
            _spriteBatch.Draw(_backgroundColor, DrawAreaWithParentOffset, Color.FromNonPremultiplied(255, 255, 255, 16));
        }

        if (Style == ListItemStyle.Large)
        {
            if (ShowIconBackGround)
            {
                _spriteBatch.Draw(_gfxPadThing, DrawPositionWithParentOffset + GetCoordsFromGraphic(_gfxPadThing.Bounds), Color.White);
            }

            if (IconGraphic != null)
            {
                var graphicOffset = IconGraphicSource.HasValue ? GetCoordsFromGraphic(IconGraphicSource.Value) : GetCoordsFromGraphic(IconGraphic.Bounds);
                _spriteBatch.Draw(IconGraphic, DrawPositionWithParentOffset + graphicOffset, IconGraphicSource, Color.White);
            }
        }

        _spriteBatch.End();
    }

    protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
    {
        if (eventArgs.Button == MouseButton.Left)
        {
            LeftClick?.Invoke(this, eventArgs);
        }
        else if (eventArgs.Button == MouseButton.Right)
        {
            RightClick?.Invoke(this, eventArgs);
        }

        return true;
    }

    private static Vector2 GetCoordsFromGraphic(Rectangle sourceTextureArea)
    {
        return new Vector2((float)Math.Round((56 - sourceTextureArea.Width) / 2f), (float)Math.Round((36 - sourceTextureArea.Height) / 2f));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _backgroundColor.Dispose();

        base.Dispose(disposing);
    }
}