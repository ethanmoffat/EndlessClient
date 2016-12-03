// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.HUD.Panels.Old;
using EndlessClient.Old;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls.Old;

namespace EndlessClient.UIControls
{
    public enum ScrollBarColors
    {
        LightOnDark, //bottom set of light
        LightOnLight, //top set of light
        LightOnMed, //middle set of light
        DarkOnDark //very bottom set
    }

    public class OldScrollBar : XNAControl
    {
        private Rectangle scrollArea; //area valid for scrolling: always 16 from top and 16 from bottom
        public int ScrollOffset { get; private set; }
        public int LinesToRender { get; set; }

        private readonly XNAButton up, down, scroll; //buttons

        private int _totalHeight;

        public OldScrollBar(XNAControl parent,
            Vector2 locationRelativeToParent,
            Vector2 size,
            ScrollBarColors palette)
            : this(parent, locationRelativeToParent, size, palette, EOGame.Instance.GFXManager) { }

        public OldScrollBar(XNAControl parent,
            Vector2 locationRelaiveToParent,
            Vector2 size,
            ScrollBarColors palette,
            INativeGraphicsManager nativeGraphicsManager)
            : base(locationRelaiveToParent,
                   new Rectangle((int)locationRelaiveToParent.X,
                                 (int)locationRelaiveToParent.Y,
                                 (int)size.X,
                                 (int)size.Y))
        {
            SetParent(parent);
            scrollArea = new Rectangle(0, 15, 0, (int)size.Y - 15);
            DrawLocation = locationRelaiveToParent;
            ScrollOffset = 0;

            Texture2D scrollSpriteSheet = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 29);
            Rectangle[] upArrows = new Rectangle[2];
            Rectangle[] downArrows = new Rectangle[2];
            int vertOff;
            switch (palette)
            {
                case ScrollBarColors.LightOnLight: vertOff = 0; break;
                case ScrollBarColors.LightOnMed: vertOff = 105; break;
                case ScrollBarColors.LightOnDark: vertOff = 180; break;
                case ScrollBarColors.DarkOnDark: vertOff = 255; break;
                default:
                    throw new ArgumentOutOfRangeException("palette");
            }

            //regions based on verticle offset (which is based on the chosen palette)
            upArrows[0] = new Rectangle(0, vertOff + 15 * 3, 16, 15);
            upArrows[1] = new Rectangle(0, vertOff + 15 * 4, 16, 15);
            downArrows[0] = new Rectangle(0, vertOff + 15, 16, 15);
            downArrows[1] = new Rectangle(0, vertOff + 15 * 2, 16, 15);
            Rectangle scrollBox = new Rectangle(0, vertOff, 16, 15);

            Texture2D[] upButton = new Texture2D[2];
            Texture2D[] downButton = new Texture2D[2];
            Texture2D[] scrollButton = new Texture2D[2];
            for (int i = 0; i < 2; ++i)
            {
                upButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, upArrows[i].Width, upArrows[i].Height);
                Color[] upData = new Color[upArrows[i].Width * upArrows[i].Height];
                scrollSpriteSheet.GetData(0, upArrows[i], upData, 0, upData.Length);
                upButton[i].SetData(upData);

                downButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, downArrows[i].Width, downArrows[i].Height);
                Color[] downData = new Color[downArrows[i].Width * downArrows[i].Height];
                scrollSpriteSheet.GetData(0, downArrows[i], downData, 0, downData.Length);
                downButton[i].SetData(downData);

                //same texture for hover, AFAIK
                scrollButton[i] = new Texture2D(scrollSpriteSheet.GraphicsDevice, scrollBox.Width, scrollBox.Height);
                Color[] scrollData = new Color[scrollBox.Width * scrollBox.Height];
                scrollSpriteSheet.GetData(0, scrollBox, scrollData, 0, scrollData.Length);
                scrollButton[i].SetData(scrollData);
            }

            up = new XNAButton(upButton, new Vector2(0, 0));
            up.OnClick += arrowClicked;
            up.SetParent(this);
            down = new XNAButton(downButton, new Vector2(0, size.Y - 15)); //update coordinates!!!!
            down.OnClick += arrowClicked;
            down.SetParent(this);
            scroll = new XNAButton(scrollButton, new Vector2(0, 15)); //update coordinates!!!!
            scroll.OnClickDrag += scrollDragged;
            scroll.OnMouseEnter += (o, e) => { SuppressParentClickDrag(true); };
            scroll.OnMouseLeave += (o, e) => { SuppressParentClickDrag(false); };
            scroll.SetParent(this);

            _totalHeight = DrawAreaWithOffset.Height;
        }

        public new void IgnoreDialog(Type t)
        {
            base.IgnoreDialog(t);
            up.IgnoreDialog(t);
            down.IgnoreDialog(t);
            scroll.IgnoreDialog(t);
        }

        public void UpdateDimensions(int numberOfLines)
        {
            _totalHeight = numberOfLines;
        }

        public void ScrollToTop()
        {
            ScrollOffset = 0;
            float pixelsPerLine = (float)(scrollArea.Height - scroll.DrawArea.Height * 2) / (_totalHeight - LinesToRender);
            scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scroll.DrawArea.Height + pixelsPerLine * ScrollOffset);
        }

        public void ScrollToEnd()
        {
            while (ScrollOffset < _totalHeight - LinesToRender)
                arrowClicked(down, new EventArgs());
        }

        public void SetScrollOffset(int offset)
        {
            ScrollOffset = offset;
        }

        public void SetDownArrowFlashSpeed(int milliseconds)
        {
            down.FlashSpeed = milliseconds;
        }

        //the point of arrowClicked and scrollDragged is to respond to input on the three buttons in such
        //     a way that ScrollOffset is updated and the Y coordinate for the scroll box is updated.
        //     ScrollOffset provides a value that is used within the EOScrollDialog.Draw method.
        //     The Y coordinate for the scroll box determines where it is drawn.
        private void arrowClicked(object btn, EventArgs e)
        {
            //_totalHeight contains the number of lines to render
            //any less than LinesToRender shouldn't scroll
            if (_totalHeight <= LinesToRender)
                return;

            if (btn == up)
            {
                if (ScrollOffset > 0)
                    ScrollOffset--;
                else
                    return;
            }
            else if (btn == down)
            {
                if (down.FlashSpeed.HasValue)
                    down.FlashSpeed = null; //as soon as it is clicked, stop flashing

                if (ScrollOffset < _totalHeight - LinesToRender)
                    ScrollOffset++;
                else
                    return;
            }
            else
            {
                return;
            }

            float pixelsPerLine = (float)(scrollArea.Height - scroll.DrawArea.Height * 2) / (_totalHeight - LinesToRender);
            scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scroll.DrawArea.Height + pixelsPerLine * ScrollOffset);
            if (scroll.DrawLocation.Y > scrollArea.Height - scroll.DrawArea.Height)
            {
                scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scrollArea.Height - scroll.DrawArea.Height);
            }
        }

        private void scrollDragged(object btn, EventArgs e)
        {
            if (down.FlashSpeed.HasValue)
                down.FlashSpeed = null; //as soon as we are dragged, stop flashing

            int y = Mouse.GetState().Y - (DrawAreaWithOffset.Y + scroll.DrawArea.Height / 2);

            if (y < up.DrawAreaWithOffset.Height)
                y = up.DrawAreaWithOffset.Height + 1;
            else if (y > scrollArea.Height - scroll.DrawArea.Height)
                y = scrollArea.Height - scroll.DrawArea.Height;

            scroll.DrawLocation = new Vector2(0, y);

            if (_totalHeight <= LinesToRender)
                return;

            double pixelsPerLine = (double)(scrollArea.Height - scroll.DrawArea.Height * 2) / (_totalHeight - LinesToRender);
            ScrollOffset = (int)Math.Round((y - scroll.DrawArea.Height) / pixelsPerLine);
        }

        public override void Update(GameTime gt)
        {
            if ((parent != null && !parent.Visible) || !ShouldUpdate())
                return;

            //handle mouse wheel scrolling, but only if the cursor is over the parent control of the scroll bar
            MouseState currentState = Mouse.GetState();

            //scroll wheel will only work for news because it is constructed with a panel as the parent
            //so for all other tabs, need to get the tab that it is being rendered in for mouseover to work properly
            XNAControl tempParent;
            if (parent is OldChatRenderer)
                tempParent = parent.GetParent();
            else
                tempParent = parent;

            if (currentState.ScrollWheelValue != PreviousMouseState.ScrollWheelValue
                && tempParent != null && tempParent.MouseOver && tempParent.MouseOverPreviously
                && _totalHeight > LinesToRender)
            {
                int dif = (currentState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) / -120; //otherwise you get "Natural" scroll. We'll have none of that.
                if ((dif < 0 && dif + ScrollOffset >= 0) || (dif > 0 && ScrollOffset + dif <= _totalHeight - LinesToRender))
                {
                    ScrollOffset += dif;
                    float pixelsPerLine = (float)(scrollArea.Height - scroll.DrawArea.Height * 2) /
                                          (_totalHeight - LinesToRender);
                    scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scroll.DrawArea.Height + pixelsPerLine * ScrollOffset);
                    if (scroll.DrawLocation.Y > scrollArea.Height - scroll.DrawArea.Height)
                    {
                        scroll.DrawLocation = new Vector2(scroll.DrawLocation.X, scrollArea.Height - scroll.DrawArea.Height);
                    }
                }
            }

            base.Update(gt);
        }

        public override void Draw(GameTime gt)
        {
            if ((parent != null && !parent.Visible) || !Visible)
                return;
            base.Draw(gt);
        }
    }
}
