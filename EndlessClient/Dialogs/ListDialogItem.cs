using System;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class ListDialogItem : XNAControl
    {
        private int _index;
        private int _xOffset, _yOffset;

        private IXNALabel _primaryText;
        private IXNALabel _subText;

        private readonly Texture2D _gfxPadThing;
        private readonly Texture2D _backgroundColor;

        private readonly ScrollingListDialog _parentList;

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
                _subText.ResizeBasedOnText();
            }
        }

        public Texture2D IconGraphic { get; set; }

        public bool ShowIconBackGround { get; set; }

        public event EventHandler RightClick;
        public event EventHandler LeftClick;

        public enum ListItemStyle
        {
            Small,
            Large
        }

        public ListDialogItem(ScrollingListDialog parent, ListItemStyle style, int listIndex = -1)
        {
            _parentList = parent;

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

            _subText = new XNALabel(Constants.FontSize08pt5)
            {
                AutoSize = true,
                BackColor = _primaryText.BackColor,
                ForeColor = _primaryText.ForeColor,
                DrawPosition = new Vector2(56, 20),
                Text = " ",
                Visible = Style == ListItemStyle.Large
            };
            _subText.ResizeBasedOnText();
            _subText.SetParentControl(this);
            _subText.Initialize();

            _gfxPadThing = parent.GraphicsManager.TextureFromResource(GFXTypes.MapTiles, 0, true);
            ShowIconBackGround = Style == ListItemStyle.Large;

            _backgroundColor = new Texture2D(Game.GraphicsDevice, 1, 1);
            _backgroundColor.SetData(new[] { Color.White });

            SetParentControl(parent);

            OffsetY = Style == ListItemStyle.Large ? 25 : 45;
        }

        public void SetPrimaryClickAction(EventHandler onClickAction)
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
                Underline = true
            };
            _primaryText.ResizeBasedOnText();

            ((XNAHyperLink)_primaryText).OnClick += onClickAction;

            _primaryText.SetParentControl(this);
            _primaryText.Initialize();

            oldText.Dispose();

            if (Style == ListItemStyle.Small)
                LeftClick += onClickAction;
        }

        public void SetSubtextClickAction(EventHandler onClickAction)
        {
            if (Style == ListItemStyle.Small)
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
                Underline = true
            };
            _subText.ResizeBasedOnText();

            ((XNAHyperLink)_subText).OnClick += onClickAction;

            _subText.SetParentControl(this);
            _subText.Initialize();

            oldText.Dispose();
        }

        public void Highlight()
        {
            _primaryText.ForeColor = Color.FromNonPremultiplied(0xf0, 0xf0, 0xf0, 0xff);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            base.OnUpdateControl(gameTime);

            if (MouseOver && MouseOverPreviously)
            {
                _drawBackground = true;
                if (CurrentMouseState.RightButton == ButtonState.Released &&
                    PreviousMouseState.RightButton == ButtonState.Pressed &&
                    !_parentList.ChildControlClickHandled)
                {
                    RightClick?.Invoke(this, EventArgs.Empty);
                    _parentList.ChildControlClickHandled = true;
                }
                else if(CurrentMouseState.LeftButton == ButtonState.Released &&
                        PreviousMouseState.LeftButton == ButtonState.Pressed)
                {
                    // todo: this might cause the click event to be fired twice, need to double check it
                    if (_subText is XNAHyperLink && _subText.MouseOver)
                        ((XNAHyperLink)_subText).Click();
                    else
                        LeftClick?.Invoke(this, EventArgs.Empty);

                    _parentList.ChildControlClickHandled = true;
                }
            }
            else
            {
                _drawBackground = false;
            }
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            _spriteBatch.Begin();
            if (_drawBackground)
            {
                _spriteBatch.Draw(_backgroundColor, DrawAreaWithParentOffset, Color.FromNonPremultiplied(255, 255, 255, 64));
            }

            if (Style == ListItemStyle.Large)
            {
                var offset = new Vector2(OffsetX + 14, OffsetY + 36 * Index);

                if (ShowIconBackGround)
                {
                    _spriteBatch.Draw(_gfxPadThing, DrawPositionWithParentOffset + offset + GetCoordsFromGraphic(_gfxPadThing), Color.White);
                }

                if (IconGraphic != null)
                {
                    _spriteBatch.Draw(IconGraphic, DrawPositionWithParentOffset + offset + GetCoordsFromGraphic(IconGraphic), Color.White);
                }
            }

            _spriteBatch.End();
        }

        private static Vector2 GetCoordsFromGraphic(Texture2D sourceTexture)
        {
            return new Vector2((float)Math.Round((64 - sourceTexture.Width) / 2f), (float)Math.Round((36 - sourceTexture.Height) / 2f));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _backgroundColor.Dispose();

            base.Dispose(disposing);
        }
    }
}
