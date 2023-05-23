using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using XNAControls;
using static EndlessClient.Dialogs.QuestStatusListDialogItem;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace EndlessClient.Dialogs
{
    public class BookDialog : PlayerInfoDialog
    {
        private readonly IPaperdollProvider _paperdollProvider;

        private readonly List<XNALabel> _childItems;
        private ScrollBar _scrollBar;

        private int _lastScrollOffset;

        public BookDialog(INativeGraphicsManager graphicsManager,
                          IEODialogButtonService eoDialogButtonService,
                          IPubFileProvider pubFileProvider,
                          IPaperdollProvider paperdollProvider,
                          Character character,
                          bool isMainCharacter)
            : base(graphicsManager, eoDialogButtonService, pubFileProvider, paperdollProvider, character, isMainCharacter)
        {
            _paperdollProvider = paperdollProvider;

            _childItems = new List<XNALabel>();

            var backgroundTexture = graphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27);
            _scrollBar = new ScrollBar(new Vector2(188, 34), backgroundTexture, new Rectangle(303, 2, 20, 237), ScrollBarColors.DarkOnDark, graphicsManager)
            {
                LinesToRender = 14
            };
            _scrollBar.SetParentControl(this);
            SetScrollWheelHandler(_scrollBar);

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 69);

            CenterInGameView();

            if (!Game.Window.AllowUserResizing)
                DrawPosition = new Vector2(DrawPosition.X, 15);
        }

        public override void Initialize()
        {
            _scrollBar.Initialize();

            base.Initialize();
        }

        protected override void OnUnconditionalUpdateControl(GameTime gameTime)
        {
            if (_childItems.Count > _scrollBar.LinesToRender && _lastScrollOffset != _scrollBar.ScrollOffset)
            {
                _lastScrollOffset = _scrollBar.ScrollOffset;

                for (int i = 0; i < _childItems.Count; i++)
                {
                    _childItems[i].DrawPosition = new Vector2(_childItems[i].DrawPosition.X, 42 + (i - _lastScrollOffset) * 16);
                    _childItems[i].Visible = (i - _lastScrollOffset) >= 0 && (i - _lastScrollOffset) < _scrollBar.LinesToRender;
                }
            }

            base.OnUnconditionalUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            var iconTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 68, true);

            _spriteBatch.Begin();

            for (int i = 0; i < Math.Min(_childItems.Count, _scrollBar.LinesToRender); i++)
            {
                _spriteBatch.Draw(iconTexture, DrawPositionWithParentOffset + new Vector2(26, 41 + i * 16), GetIconSourceRectangle(QuestStatusIcon.None2), Color.White);
            }

            _spriteBatch.End();
        }

        protected override void UpdateDisplayedData(PaperdollData paperdollData)
        {
            base.UpdateDisplayedData(paperdollData);

            foreach (var item in _childItems)
                item.Dispose();

            _childItems.Clear();

            for (int i = 0; i < paperdollData.QuestNames.Count; i++)
            {
                var quest = paperdollData.QuestNames[i];

                var nextLabel = new XNALabel(Constants.FontSize08pt5)
                {
                    Text = quest,
                    ForeColor = ColorConstants.LightGrayText,
                    AutoSize = true,
                    DrawPosition = new Vector2(50, 42 + i * 16),
                    Visible = i < _scrollBar.LinesToRender
                };
                nextLabel.SetScrollWheelHandler(_scrollBar);
                nextLabel.ResizeBasedOnText();
                nextLabel.SetParentControl(this);
                nextLabel.Initialize();

                _childItems.Add(nextLabel);
            }

            _scrollBar.ScrollToTop();
            _scrollBar.UpdateDimensions(paperdollData.QuestNames.Count);
        }

        // copied from QuestStatusListDialogItem
        private static Rectangle GetIconSourceRectangle(QuestStatusIcon index)
        {
            return new Rectangle((int)index * 15, 0, 15, 15);
        }
    }
}
