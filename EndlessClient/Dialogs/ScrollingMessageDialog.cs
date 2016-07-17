// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class ScrollingMessageDialog : EODialogBase
    {
        private readonly ScrollBar _scrollBar;
        private readonly List<string> _chatStrings = new List<string>();
        private readonly TextSplitter _textSplitter;
        private readonly SpriteFont _font;

        public new string MessageText
        {
            set
            {
                _chatStrings.Clear();
                _textSplitter.Text = value;

                //special case: blank line, like in the news panel between news items
                if (string.IsNullOrWhiteSpace(value))
                {
                    _chatStrings.Add(" ");
                    _scrollBar.UpdateDimensions(_chatStrings.Count);
                    return;
                }

                //don't do multi-line processing if we don't need to
                if (!_textSplitter.NeedsProcessing)
                {
                    _chatStrings.Add(value);
                    _scrollBar.UpdateDimensions(_chatStrings.Count);
                    return;
                }

                _chatStrings.AddRange(_textSplitter.SplitIntoLines());

                _scrollBar.UpdateDimensions(_chatStrings.Count);
                _scrollBar.LinesToRender = (int)Math.Round(110.0f / 13); //draw area for the text is 117px, 13px per line
                if (_scrollBar.LinesToRender < _chatStrings.Count)
                    _scrollBar.SetDownArrowFlashSpeed(500);
            }
        }

        public ScrollingMessageDialog(INativeGraphicsManager gfxManager,
                                      IGraphicsDeviceProvider graphicsDeviceProvider,
                                      IGameStateProvider gameStateProvider)
            : base(gfxManager)
        {
            _font = Game.Content.Load<SpriteFont>(Constants.FontSize08);
            _textSplitter = new TextSplitter("", _font) { LineLength = 275 };

            bgTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 40);
            _setSize(bgTexture.Width, bgTexture.Height);

            var ok = new XNAButton(smallButtonSheet, new Vector2(138, 197), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
            ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
            ok.SetParent(this);
            dlgButtons.Add(ok);

            _scrollBar = new ScrollBar(this,
                new Vector2(320, 66),
                new Vector2(16, 119),
                ScrollBarColors.LightOnMed,
                gfxManager);
            MessageText = "";

            CenterAndFixDrawOrder(graphicsDeviceProvider, gameStateProvider);
        }

        public override void Draw(GameTime gt)
        {
            if ((parent != null && !parent.Visible) || !Visible)
                return;

            base.Draw(gt);
            if (_scrollBar == null) return; //prevent nullreferenceexceptions

            SpriteBatch.Begin();
            var pos = new Vector2(27 + (int)DrawLocation.X, 69 + (int)DrawLocation.Y);

            for (int i = _scrollBar.ScrollOffset; i < _scrollBar.ScrollOffset + _scrollBar.LinesToRender; ++i)
            {
                if (i >= _chatStrings.Count)
                    break;

                var strToDraw = _chatStrings[i];

                SpriteBatch.DrawString(_font ?? EOGame.Instance.DBGFont, strToDraw, new Vector2(pos.X, pos.Y + (i - _scrollBar.ScrollOffset) * 13), ColorConstants.LightGrayText);
            }

            SpriteBatch.End();
        }
    }
}
