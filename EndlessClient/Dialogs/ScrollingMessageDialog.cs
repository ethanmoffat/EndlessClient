// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class ScrollingMessageDialog : BaseEODialog
    {
        private readonly XNAButton _ok;
        private readonly ScrollBar _scrollBar;
        private readonly List<string> _chatStrings = new List<string>();
        private readonly TextSplitter _textSplitter;
        private readonly SpriteFont _font;

        public string MessageText
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

        public ScrollingMessageDialog(INativeGraphicsManager nativeGraphicsManager,
                                      IGameStateProvider gameStateProvider,
                                      IEODialogButtonService eoDialogButtonService)
            : base(gameStateProvider)
        {
            _font = Game.Content.Load<SpriteFont>(Constants.FontSize08);
            _textSplitter = new TextSplitter("", _font) { LineLength = 275 };

            BackgroundTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 40);

            var smallButtonSheet = eoDialogButtonService.SmallButtonSheet;

            _ok = new XNAButton(smallButtonSheet,
                new Vector2(138, 197),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            _ok.OnClick += (sender, e) => Close(XNADialogResult.OK);
            _ok.SetParentControl(this);

            _scrollBar = new ScrollBar(new Vector2(320, 66), new Vector2(16, 119),
                ScrollBarColors.LightOnMed, nativeGraphicsManager);
            _scrollBar.SetParentControl(this);

            MessageText = "";

            CenterInGameView();
        }

        public override void Initialize()
        {
            _ok.Initialize();
            _scrollBar.Initialize();

            base.Initialize();
        }

        protected override void OnDrawControl(GameTime gt)
        {
            base.OnDrawControl(gt);

            _spriteBatch.Begin();
            var pos = new Vector2(27 + (int)DrawPosition.X, 69 + (int)DrawPosition.Y);

            for (int i = _scrollBar.ScrollOffset; i < _scrollBar.ScrollOffset + _scrollBar.LinesToRender; ++i)
            {
                if (i >= _chatStrings.Count)
                    break;

                var strToDraw = _chatStrings[i];

                _spriteBatch.DrawString(_font, strToDraw, new Vector2(pos.X, pos.Y + (i - _scrollBar.ScrollOffset) * 13), ColorConstants.LightGrayText);
            }

            _spriteBatch.End();
        }
    }
}
