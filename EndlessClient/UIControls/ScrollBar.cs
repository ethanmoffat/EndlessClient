using System;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.UIControls
{
    public enum ScrollBarColors
    {
        LightOnDark, //bottom set of light
        LightOnLight, //top set of light
        LightOnMed, //middle set of light
        DarkOnDark //very bottom set
    }

    public class ScrollBar : XNAControl
    {
        private Rectangle scrollArea; //area valid for scrolling: always 16 from top and 16 from bottom
        public int ScrollOffset { get; private set; }
        public int LinesToRender { get; set; }

        private readonly XNAButton _upButton, _downButton, _scrollButton; //buttons

        private int _totalHeight;

        public ScrollBar(Vector2 locationRelativeToParent,
                         Vector2 size,
                         ScrollBarColors palette,
                         INativeGraphicsManager nativeGraphicsManager)
        {
            scrollArea = new Rectangle(0, 15, 0, (int)size.Y - 15);
            DrawPosition = locationRelativeToParent;
            SetSize((int)size.X, (int)size.Y);
            ScrollOffset = 0;

            var scrollSpriteSheet = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 29);
            var upArrows = new Rectangle[2];
            var downArrows = new Rectangle[2];
            int vertOff;
            switch (palette)
            {
                case ScrollBarColors.LightOnLight: vertOff = 0; break;
                case ScrollBarColors.LightOnMed: vertOff = 105; break;
                case ScrollBarColors.LightOnDark: vertOff = 180; break;
                case ScrollBarColors.DarkOnDark: vertOff = 255; break;
                default: throw new ArgumentOutOfRangeException(nameof(palette));
            }

            //regions based on verticle offset (which is based on the chosen palette)
            upArrows[0] = new Rectangle(0, vertOff + 15 * 3, 16, 15);
            upArrows[1] = new Rectangle(0, vertOff + 15 * 4, 16, 15);
            downArrows[0] = new Rectangle(0, vertOff + 15, 16, 15);
            downArrows[1] = new Rectangle(0, vertOff + 15 * 2, 16, 15);
            var scrollBox = new Rectangle(0, vertOff, 16, 15);

            _upButton = new XNAButton(scrollSpriteSheet, Vector2.Zero, upArrows[0], upArrows[1]);
            _upButton.OnClick += arrowClicked;
            _upButton.SetParentControl(this);

            _downButton = new XNAButton(scrollSpriteSheet, new Vector2(0, size.Y - 15), downArrows[0], downArrows[1]);
            _downButton.OnClick += arrowClicked;
            _downButton.SetParentControl(this);

            _scrollButton = new XNAButton(scrollSpriteSheet, new Vector2(0, 15), scrollBox, scrollBox);
            _scrollButton.OnClickDrag += OnScrollButtonDragged;
            _scrollButton.OnMouseEnter += (o, e) => { SuppressParentClickDragEvent(true); };
            _scrollButton.OnMouseLeave += (o, e) => { SuppressParentClickDragEvent(false); };
            _scrollButton.SetParentControl(this);

            _totalHeight = DrawAreaWithParentOffset.Height;
        }

        public override void Initialize()
        {
            _upButton.Initialize();
            _downButton.Initialize();
            _scrollButton.Initialize();

            base.Initialize();
        }

        public void UpdateDimensions(int numberOfLines)
        {
            _totalHeight = numberOfLines;
        }

        public void ScrollToTop()
        {
            ScrollOffset = 0;
            var pixelsPerLine = (float)(scrollArea.Height - _scrollButton.DrawArea.Height * 2) / (_totalHeight - LinesToRender);
            _scrollButton.DrawPosition = new Vector2(_scrollButton.DrawArea.X, _scrollButton.DrawArea.Height + pixelsPerLine * ScrollOffset);
        }

        public void ScrollToEnd()
        {
            while (ScrollOffset < _totalHeight - LinesToRender)
                arrowClicked(_downButton, new EventArgs());
        }

        public void SetScrollOffset(int offset)
        {
            ScrollOffset = offset;
        }

        public void SetDownArrowFlashSpeed(int milliseconds)
        {
            _downButton.FlashSpeed = milliseconds;
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

            if (btn == _upButton)
            {
                if (ScrollOffset > 0)
                    ScrollOffset--;
                else
                    return;
            }
            else if (btn == _downButton)
            {
                if (_downButton.FlashSpeed.HasValue)
                    _downButton.FlashSpeed = null; //as soon as it is clicked, stop flashing

                if (ScrollOffset < _totalHeight - LinesToRender)
                    ScrollOffset++;
                else
                    return;
            }
            else
            {
                return;
            }

            var pixelsPerLine = (float)(scrollArea.Height - _scrollButton.DrawArea.Height * 2) / (_totalHeight - LinesToRender);
            _scrollButton.DrawPosition = new Vector2(_scrollButton.DrawPosition.X, _scrollButton.DrawArea.Height + pixelsPerLine * ScrollOffset);
            if (_scrollButton.DrawPosition.Y > scrollArea.Height - _scrollButton.DrawArea.Height)
            {
                _scrollButton.DrawPosition = new Vector2(_scrollButton.DrawPosition.X, scrollArea.Height - _scrollButton.DrawArea.Height);
            }
        }

        private void OnScrollButtonDragged(object btn, EventArgs e)
        {
            if (_downButton.FlashSpeed.HasValue)
                _downButton.FlashSpeed = null; //as soon as we are dragged, stop flashing

            var y = CurrentMouseState.Y - (DrawAreaWithParentOffset.Y + _scrollButton.DrawArea.Height / 2);

            if (y < _upButton.DrawAreaWithParentOffset.Height)
                y = _upButton.DrawAreaWithParentOffset.Height + 1;
            else if (y > scrollArea.Height - _scrollButton.DrawArea.Height)
                y = scrollArea.Height - _scrollButton.DrawArea.Height;

            _scrollButton.DrawPosition = new Vector2(0, y);

            if (_totalHeight <= LinesToRender)
                return;

            var pixelsPerLine = (double)(scrollArea.Height - _scrollButton.DrawArea.Height * 2) / (_totalHeight - LinesToRender);
            ScrollOffset = (int)Math.Round((y - _scrollButton.DrawArea.Height) / pixelsPerLine);
        }

        protected override void OnUpdateControl(GameTime gt)
        {
            if (CurrentMouseState.ScrollWheelValue != PreviousMouseState.ScrollWheelValue
                && ImmediateParent != null && ImmediateParent.MouseOver && ImmediateParent.MouseOverPreviously
                && _totalHeight > LinesToRender)
            {
                //value must be /-120, otherwise you get "Natural" scroll. We'll have none of that.
                var dif = (CurrentMouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) / -120;
                if ((dif < 0 && dif + ScrollOffset >= 0) || (dif > 0 && ScrollOffset + dif <= _totalHeight - LinesToRender))
                {
                    ScrollOffset += dif;

                    var pixelsPerLine = (float)(scrollArea.Height - _scrollButton.DrawArea.Height * 2) /
                                        (_totalHeight - LinesToRender);
                    _scrollButton.DrawPosition = new Vector2(_scrollButton.DrawPosition.X, _scrollButton.DrawArea.Height + pixelsPerLine * ScrollOffset);

                    if (_scrollButton.DrawPosition.Y > scrollArea.Height - _scrollButton.DrawArea.Height)
                        _scrollButton.DrawPosition = new Vector2(_scrollButton.DrawPosition.X, scrollArea.Height - _scrollButton.DrawArea.Height);
                }
            }

            base.OnUpdateControl(gt);
        }
    }
}
